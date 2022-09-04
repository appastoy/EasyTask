using System;

namespace EasyTask
{
    partial struct ETask
    {
        public static async ETask Run(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            await SwitchToThreadPool();
            action.Invoke();
        }

        public static async ETask Run(Func<ETask> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            await SwitchToThreadPool();
            await func.Invoke();
        }

        public static async ETask<T> Run<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            await SwitchToThreadPool();
            return func.Invoke();
        }

        public static async ETask<T> Run<T>(Func<ETask<T>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            await SwitchToThreadPool();
            return await func.Invoke();
        }
    }
}
