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

                public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

                public void UnsafeOnCompleted(Action continuation)
                    => ThreadPool.QueueUserWorkItem(DelegateCache.InvokeAsActionT, continuation, false);
            }
        }
    }
}
