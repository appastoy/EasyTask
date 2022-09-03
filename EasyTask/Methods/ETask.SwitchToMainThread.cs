using System;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        static SynchronizationContext? mainThreadContext;
        public static SynchronizationContext? MainThreadContext => mainThreadContext;

        public static void SetMainThreadContext(SynchronizationContext? context)
            => mainThreadContext = context;
        
        public static SwitchSynchronizationContextAwaitable SwitchToMainThread()
        {
            if (mainThreadContext == null)
                throw new InvalidOperationException($"Main Thread SynchronizationContext is null. You should it to call ETask.{nameof(SetMainThreadContext)}().");

            return new SwitchSynchronizationContextAwaitable(mainThreadContext);
        }
    }
}
