using EasyTask.Helpers;
using EasyTask.Pools;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace EasyTask.Sources
{
    internal interface IConfigureAwaitable
    {
        void SetCaptureContext(bool captureContext);
    }

    public abstract class ETaskSourceBase<T> : PoolItem<T>, IConfigureAwaitable
        where T : ETaskSourceBase<T>, new()
    {
        short token;
        Action<object> continuation;
        object state;
        int completedCheck;
        object? error;
        protected SynchronizationContext? context;

        public short Token
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => token;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void BeforeRent() => context = SynchronizationContext.Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IConfigureAwaitable.SetCaptureContext(bool captureContext)
        {
            if (captureContext)
                context = SynchronizationContext.Current;
            else
                context = null;
        }

        protected override void Reset()
        {
            try
            {
                ReportUnhandledError();
            }
            finally
            {
                unchecked { token += 1; }

                continuation = null;
                state = null;
                error = null;
                context = null;
                completedCheck = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReportUnhandledError()
        {
            if (error is ExceptionHolder holder)
                ETask.PublishUnhandledException(holder.GetException());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void GetResultInternal(short token)
        {
            ValidateToken(token);
            while (completedCheck == 0)
                Thread.Yield();
            ThrowIfHasError();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrySetException(Exception exception)
        {
            if (IsCompletedFirst())
            {
                error = exception is OperationCanceledException oce ? oce :
                        new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                InvokeContinuation();
            }
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrySetCanceled(OperationCanceledException operationCanceledException)
        {
            if (IsCompletedFirst())
            {
                error = operationCanceledException;
                InvokeContinuation();
            }
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ETaskStatus GetStatus(short token)
        {
            ValidateToken(token);
            return completedCheck == 0 ? ETaskStatus.Pending :
                   error == null ? ETaskStatus.Succeeded :
                   error.GetType() == typeof(OperationCanceledException) ? ETaskStatus.Canceled :
                   ETaskStatus.Faulted;
        }

        [DebuggerHidden]
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
            PostOrInvokeContinuation(continuation, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ValidateToken(short token)
        {
            if (this.token != token)
                throw new InvalidOperationException("Invalid token");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void InvokeContinuation()
        {
            if (continuation != null || Interlocked.CompareExchange(ref continuation, DelegateCache.InvokeNoop, null) != null)
            {
                Action<object> tempContinuation = continuation;
                object tempState = state;
                continuation = null;
                state = null;
                PostOrInvokeContinuation(tempContinuation, tempState);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PostOrInvokeContinuation(Action<object> continuation, object state)
        {
            if (context == null || 
                context == SynchronizationContext.Current)
                continuation.Invoke(state);
            else
                context.Post(DelegateCache.InvokeContinuationWithState, TuplePool.Rent(continuation, state));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCompletedFirst() => Interlocked.Increment(ref completedCheck) == 1;
    }
}
