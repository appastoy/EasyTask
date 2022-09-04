using EasyTask.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchSynchronizationContextAwaitable SwitchSynchronizationContext(SynchronizationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return new SwitchSynchronizationContextAwaitable(context);
        }

        public readonly struct SwitchSynchronizationContextAwaitable
        {
            readonly SynchronizationContext context;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal SwitchSynchronizationContextAwaitable(SynchronizationContext context)
                => this.context = context;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Awaiter GetAwaiter() => new (context);

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                readonly SynchronizationContext context;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Awaiter(SynchronizationContext context)
                    => this.context = context;

                public bool IsCompleted
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => context != null &&
                           context == SynchronizationContext.Current;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void GetResult() { }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void OnCompleted(Action continuation)
                    => context.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void UnsafeOnCompleted(Action continuation)
                    => context.Post(DelegateCache.InvokeAsSendOrPostCallback, continuation);
            }
        }
    }
}
