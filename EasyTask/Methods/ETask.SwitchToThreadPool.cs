using EasyTask.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Switch to thread pool.(switch to other thread from current thread.)
        /// </summary>
        /// <returns>Awaitable</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchToThreadPoolAwaitable SwitchToThreadPool() => default;

        public readonly struct SwitchToThreadPoolAwaitable
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Awaiter GetAwaiter() => default;

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                public bool IsCompleted
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => false;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void GetResult() { }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void OnCompleted(Action continuation)
                    => ThreadPool.QueueUserWorkItem(DelegateCache.InvokeAsActionT, continuation, false);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void UnsafeOnCompleted(Action continuation)
                    => ThreadPool.UnsafeQueueUserWorkItem(DelegateCache.InvokeAsWaitCallback, continuation);
            }
        }
    }
}
