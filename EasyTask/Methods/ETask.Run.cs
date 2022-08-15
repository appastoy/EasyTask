using System;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        static ETask RunOnThreadPool(Action action) => Run(action, true);
        static ETask RunOnThreadPool(Func<ETask> func) => Run(func, true);
        static ETask RunOnThreadPoolAndSwitchToCurrent(Action action) => Run(action, false);
        static ETask RunOnThreadPoolAndSwitchToCurrent(Func<ETask> func) => Run(func, false);

        static async ETask Run(Action action, bool captureContext = true)
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

        static async ETask Run(Func<ETask> func, bool captureContext = true)
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
