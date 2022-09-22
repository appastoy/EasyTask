using EasyTask.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Yield clock to other thread.
        /// </summary>
        /// <returns>Awaitable</returns>
        public static YieldAwaitable Yield() => default;
        public readonly struct YieldAwaitable
        {
            public Awaiter GetAwaiter() => default;

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                public bool IsCompleted
                {
                    get => false;
                }

                public void GetResult() { }

                public void OnCompleted(Action continuation)
                {
                    if (!TryPost(continuation))
                        ThreadPool.QueueUserWorkItem(DelegateCache.InvokeAsActionT, continuation, false);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    if (!TryPost(continuation))
                        ThreadPool.UnsafeQueueUserWorkItem(DelegateCache.InvokeAsWaitCallback, continuation);
                }

                static bool TryPost(Action continuation)
                {
                    var sync = SynchronizationContext.Current;
                    if (sync != null)
                    {
                        sync.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}
