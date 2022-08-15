using EasyTask.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static SwitchToThreadPoolAwaitable SwitchToThreadPool() => default;

        public readonly struct SwitchToThreadPoolAwaitable
        {
            public Awaiter GetAwaiter() => default;

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                public bool IsCompleted => false;

                public void GetResult() { }

                public void OnCompleted(Action continuation)
                    => ThreadPool.QueueUserWorkItem(DelegateCache.InvokeAsActionT, continuation, false);

                public void UnsafeOnCompleted(Action continuation)
                    => ThreadPool.UnsafeQueueUserWorkItem(DelegateCache.InvokeAsWaitCallback, continuation);
            }
        }
    }
}
