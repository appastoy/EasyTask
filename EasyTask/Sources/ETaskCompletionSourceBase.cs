using EasyTask.Helpers;
using EasyTask.Pools;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace EasyTask.Sources
{
    public abstract class ETaskCompletionSourceBase<T> : PoolItem<T>
        where T : ETaskCompletionSourceBase<T>, new()
    {
        short token;
        Action<object> continuation;
        object state;
        int completedCheck;
        object? error;

        public short Token => token;

        protected override void BeforeRent()
        {
            ReportUnhandledError();

            unchecked { token += 1; }

            continuation = null;
            state = null;
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

        protected void GetResultInternal(short token)
        {
            ValidateToken(token);
            if (Volatile.Read(ref completedCheck) == 0)
                throw new InvalidOperationException("Task is not completed.");
            ThrowIfHasError();
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
    }
}
