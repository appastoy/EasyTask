using EasyTask.Pools;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace EasyTask.Sources
{
    internal class Promise<TPromise> : PoolItem<TPromise>, IPromise
        where TPromise : Promise<TPromise>, new()
    {
        short token;
        Action<object?>? continuation;
        object? state;
        int completedCheck;
        object? error;

        public short Token => token;

        public void Reset()
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

        public void GetResult(short token)
        {
            try
            {
                ValidateToken(token);
                if (Volatile.Read(ref completedCheck) == 0)
                    throw new InvalidOperationException("Task is not completed.");
                ThrowIfHasError();
            }
            finally
            {
                Return();
            }
        }

        private void ThrowIfHasError()
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
            EnsureCompletedFirst();
            continuation?.Invoke(state);
        }

        public void TrySetException(Exception exception)
        {
            EnsureCompletedFirst();
            error = exception is OperationCanceledException oce ? (object)oce :
                    new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
            continuation?.Invoke(state);
        }

        public void TrySetCanceled(CancellationToken token = default)
        {
            EnsureCompletedFirst();
            error = new OperationCanceledException(token);
            continuation?.Invoke(state);
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

        public void OnCompleted(Action<object?>? continuation, object? state, short token)
        {
            ValidateToken(token);
            SetContinuation(continuation);
            this.state = state;
        }

        void ValidateToken(short token)
        {
            if (this.token != token)
                throw new InvalidOperationException("Invalid token");
        }

        void SetContinuation(Action<object?>? continuation)
        {
            if (Interlocked.CompareExchange(ref this.continuation, continuation, null) != null)
                throw new InvalidOperationException("Task already awaited. You should not await more than twice.");
        }

        void EnsureCompletedFirst()
        {
            if (Interlocked.Increment(ref completedCheck) != 1)
                throw new InvalidOperationException("Task already completed.");
        }
    }
}
