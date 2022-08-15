using System;
using System.Runtime.CompilerServices;

namespace EasyTask
{
    partial struct ETask
    {
        public static SwitchToMainThreadAwaitable SwitchToMainThread() => default;

        public readonly struct SwitchToMainThreadAwaitable
        {
            public Awaiter GetAwaiter() => default;

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                public bool IsCompleted => false;

                public void GetResult() { }

                public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

                public void UnsafeOnCompleted(Action continuation)
                {
                    EnsureMainThreadSynchronizationContext();
                    mainThreadSynchronizationContext?.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);
                }
            }
        }
    }
}
