using System;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        internal static SynchronizationContext mainThreadSynchronizationContext;

        static ETask() { mainThreadSynchronizationContext = SynchronizationContext.Current; }

        public static void SetMainThreadSynchronizationContext(SynchronizationContext? context)
        {
            mainThreadSynchronizationContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        internal static void EnsureMainThreadSynchronizationContext()
        {
            if (mainThreadSynchronizationContext == null)
            {
                mainThreadSynchronizationContext = SynchronizationContext.Current;
                ValidateMainThreadSynchronizationContext();
            }
        }

        internal static void ValidateMainThreadSynchronizationContext()
        {
            if (mainThreadSynchronizationContext == null)
                throw new InvalidOperationException("Main Thread SynchronizationContext is null. You should it to call ETask.SetMainThreadSynchronizationContext().");
        }
    }
}
