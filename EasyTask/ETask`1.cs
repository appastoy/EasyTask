using EasyTask.CompilerServices;
using EasyTask.Helpers;
using EasyTask.Sources;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8625
#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask
{
    /// <summary>
    /// Lightweight task object with result for less allocation.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    [AsyncMethodBuilder(typeof(ETaskMethodBuilder<>))]
    public readonly partial struct ETask<TResult>
    {
        readonly IETaskSource<TResult> source;
        readonly short token;
        readonly TResult result;

        public ETaskStatus Status
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => source?.GetStatus(token) ?? ETaskStatus.Succeeded;
        }

        /// <summary>
        /// Is Completed Successfully or Exception Raised or Canceled?
        /// </summary>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Status != ETaskStatus.Pending;
        }

        public bool IsCompletedSuccessfully
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Status == ETaskStatus.Succeeded;
        }

        /// <summary>
        /// Is Exception Raised? (except OperationCanceledException)
        /// </summary>
        public bool IsFaulted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Status == ETaskStatus.Faulted;
        }

        /// <summary>
        /// Is OperationCanceledException Raised?
        /// </summary>
        public bool IsCanceled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Status == ETaskStatus.Canceled;
        }

        /// <summary>
        /// If task is not completed, It stucks until completed. You cannot access this property [more than once] or [after awaited].
        /// </summary>
        public TResult Result
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => source != null ? source.GetResult(token) : result;
        }

        public Exception? Exception
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => source is ETask.ExceptionSource exceptionSource ? exceptionSource.GetException() :
                   source is ETask.CanceledSource canceledSource ? canceledSource.Exception :
                   source is IExceptionHolder exceptionHolder ? exceptionHolder.Exception :
                   null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ETask(IETaskSource<TResult> source, short token)
        {
            this.source = source;
            this.token = token;
            result = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ETask(TResult result)
        {
            source = default;
            token = default;
            this.result = result;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter() => new Awaiter(in this);

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            readonly ETask<TResult> task;

            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => task.IsCompleted;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Awaiter(in ETask<TResult> task) => this.task = task;

            /// <summary>
            /// This method cannot be called [more than once] or [after awaited] after the task has been created. Be aware.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TResult GetResult() => task.source == null ? task.result : task.source.GetResult(task.token);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void INotifyCompletion.OnCompleted(Action continuation)
            {
                if (task.source is null)
                    continuation.Invoke();
                else
                    OnCompleted(DelegateCache.InvokeAsActionObject, continuation);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation)
            {
                if (task.source is null)
                    continuation.Invoke();
                else
                    OnCompleted(DelegateCache.InvokeAsActionObject, continuation);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void OnCompleted(Action<object> continuation, object state)
            {
                if (task.source is null)
                    continuation.Invoke(state);
                else
                    task.source.OnCompleted(continuation, state, task.token);
            }
        }
    }
}