using System;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static async ETask Run(Action action, bool captureContext = true)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (captureContext)
            {
                var synchronizationContext = SynchronizationContext.Current;
                await SwitchToThreadPool();
                try
                {
                    action.Invoke();
                }
                finally
                {
                    if (synchronizationContext != null)
                        await SwitchSynchronizationContext(synchronizationContext);
                }
            }
            else
            {
                await SwitchToThreadPool();
                action.Invoke();
            }
        }

        public static async ETask Run(Func<ETask> func, bool captureContext = true)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (captureContext)
            {
                var synchronizationContext = SynchronizationContext.Current;
                await SwitchToThreadPool();
                try
                {
                    await func.Invoke();
                }
                finally
                {
                    if (synchronizationContext != null)
                        await SwitchSynchronizationContext(synchronizationContext);
                }
            }
            else
            {
                await SwitchToThreadPool();
                await func.Invoke();
            }
        }

    }
}
