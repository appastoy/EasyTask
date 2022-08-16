using EasyTask.Helpers;
using EasyTask.Pools;
using EasyTask.Sources;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace EasyTask.Promises
{
    internal class Promise<TPromise> : PoolItem<TPromise>, IPromise
        where TPromise : Promise<TPromise>, new()
    {
        short token;

#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
        Action<object> continuation;
        object state;
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.

        int completedCheck;
        object? error;

        public ETask Task => new ETask(this, Token);

        public short Token => token;

        protected override void BeforeRent()
        {
            ReportUnhandledError();

            unchecked { token += 1; }

#pragma warning disable CS8625 // Null 리터럴을 null을 허용하지 않는 참조 형식으로 변환할 수 없습니다.
            continuation = null;
            state = null;
#pragma warning restore CS8625 // Null 리터럴을 null을 허용하지 않는 참조 형식으로 변환할 수 없습니다.

            completedCheck = 0;
            error = null;
        }

        void ReportUnhandledError()
        {
            if (error is ExceptionHolder holder)
            {
                error = null;
                ETask.PublishUnhandledException(holder.GetException());
            }
        }

        public void GetResult(short token)
        {
            try
            {
                ValidateToken(token);
                if (!IsCompletedInternal())
                    throw new InvalidOperationException("Task is not completed.");
                ThrowIfHasError();
            }
            finally
            {
                Return();
            }
        }

        protected bool IsCompletedInternal()
        {
            return Volatile.Read(ref completedCheck) > 0;
        }

        protected void ThrowIfHasError()
        {
            if (error is null)
                return;

            try
            {
                if (error is OperationCanceledException oce)
                    throw oce;
                if (error is ExceptionHolder holder)
                    holder.GetException().Throw();
            }
            finally
            {
                error = null;
            }
        }

        public void TrySetResult()
        {
            if (IsCompletedFirst())
                InvokeContinuation();
        }

        public void TrySetException(Exception exception)
        {
            if (IsCompletedFirst())
            {
                error = exception is OperationCanceledException oce ? (object)oce :
                        new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                InvokeContinuation();
            }
        }

        public void TrySetCanceled(OperationCanceledException operationCanceledException)
        {
            if (IsCompletedFirst())
            {
                error = operationCanceledException;
                InvokeContinuation();
            }
        }

        public ETaskStatus GetStatus(short token)
        {
            ValidateToken(token);
            return completedCheck == 0 ?
                        ETaskStatus.Pending :
                   error is OperationCanceledException ?
                        ETaskStatus.Canceled :
                   error != null ?
                        ETaskStatus.Faulted :
                   ETaskStatus.Succeeded;
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            ValidateToken(token);
            SetContinuation(continuation, state);
        }

        protected void ValidateToken(short token)
        {
            if (this.token != token)
                throw new InvalidOperationException("Invalid token");
        }

        protected void InvokeContinuation()
        {
            if (continuation != null || Interlocked.CompareExchange(ref continuation, DelegateCache.InvokeNoop, null) != null)
            {
                var tempContinuation = continuation;
                var tempState = state;
                continuation = null;
                state = null;
                tempContinuation.Invoke(tempState);
            }
        }

        protected bool IsCompletedFirst() => Interlocked.Increment(ref completedCheck) == 1;

        void SetContinuation(Action<object> continuation, object state)
        {
            this.state = state;
            var oldContinuation = Interlocked.CompareExchange(ref this.continuation, continuation, null);
            if (oldContinuation == null)
                return;

            if (oldContinuation != DelegateCache.InvokeNoop)
                throw new InvalidOperationException("Task already awaited. You should not await more than twice.");

            this.continuation = null;
            this.state = null;
            continuation.Invoke(state);
        }
    }
}
