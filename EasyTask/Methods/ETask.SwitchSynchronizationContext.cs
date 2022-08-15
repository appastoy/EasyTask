using EasyTask.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static SwitchSynchronizationContextAwaitable SwitchSynchronizationContext(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException(nameof(synchronizationContext));

            return new SwitchSynchronizationContextAwaitable(synchronizationContext);
        }

        public readonly struct SwitchSynchronizationContextAwaitable
        {
            readonly SynchronizationContext synchronizationContext;

            internal SwitchSynchronizationContextAwaitable(SynchronizationContext synchronizationContext)
                => this.synchronizationContext = synchronizationContext;

            public Awaiter GetAwaiter() => new Awaiter(synchronizationContext);

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                readonly SynchronizationContext synchronizationContext;

                internal Awaiter(SynchronizationContext synchronizationContext)
                    => this.synchronizationContext = synchronizationContext;

                public bool IsCompleted
                    => synchronizationContext != null &&
                       SynchronizationContext.Current == synchronizationContext;

                public void GetResult() { }

                public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

                public void UnsafeOnCompleted(Action continuation)
                    => synchronizationContext.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);
            }
        }
    }
}
