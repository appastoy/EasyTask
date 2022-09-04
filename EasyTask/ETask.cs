using EasyTask.CompilerServices;
using EasyTask.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EasyTask
{
    /// <summary>
    /// Lightweight task object for zero allocation.
    /// </summary>
    [StructLayout(LayoutKind.Auto, Pack = 2)]
    [AsyncMethodBuilder(typeof(ETaskMethodBuilder))]
    public readonly partial struct ETask
    {
        public static readonly ETask CompletedTask = new();

        internal readonly IETaskSource source;
        internal readonly short token;

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

        public Exception? Exception
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => source is ExceptionSource exceptionSource ? exceptionSource.GetException() :
                   source is CanceledSource canceledSource ? canceledSource.Exception :
                   null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ETask(IETaskSource source, short token)
        {
            this.source = source;
            this.token = token;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter() => new(in this);

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            readonly ETask task;

            /// <summary>
            /// Is Completed Successfully or Exception Raised or Canceled?
            /// </summary>
            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => task.IsCompleted;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Awaiter(in ETask task) => this.task = task;

            /// <summary>
            /// This method cannot be called [more than once] or [after awaited] after the task has been created. Be aware.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult() => task.source?.GetResult(task.token);

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
