using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    public readonly partial struct ETask
    {
        public static YieldAwaitable Yield() => default;
        public readonly struct YieldAwaitable
        {
            public Awaiter GetAwaiter() => default;

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                public bool IsCompleted => false;

                public void GetResult() { }

                public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

                public void UnsafeOnCompleted(Action continuation)
                {
                    var sync = SynchronizationContext.Current;
                    if (sync != null)
                    {
                        sync.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(DelegateCache.InvokeAsActionT, continuation, false);
                    }
                }
            }
        }
    }
}
