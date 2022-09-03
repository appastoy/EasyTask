using EasyTask.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static SwitchSynchronizationContextAwaitable SwitchSynchronizationContext(SynchronizationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return new SwitchSynchronizationContextAwaitable(context);
        }

        public readonly struct SwitchSynchronizationContextAwaitable
        {
            readonly SynchronizationContext context;

            internal SwitchSynchronizationContextAwaitable(SynchronizationContext context)
                => this.context = context;

            public Awaiter GetAwaiter() => new (context);

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                readonly SynchronizationContext context;

                internal Awaiter(SynchronizationContext context)
                    => this.context = context;

                public bool IsCompleted
                    => context != null &&
                       context == SynchronizationContext.Current;

                public void GetResult() { }

                public void OnCompleted(Action continuation)
                    => context.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);

                public void UnsafeOnCompleted(Action continuation)
                    => context.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);
            }
        }
    }
}
