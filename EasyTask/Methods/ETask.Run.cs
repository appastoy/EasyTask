using System;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Run action on thread pool.
        /// </summary>
        /// <param name="action">action</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">action is null</exception>
        public static async ETask Run(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            await SwitchToThreadPool();
            action.Invoke();
        }

        /// <summary>
        /// Run task on thread pool.
        /// </summary>
        /// <param name="func">func</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static async ETask Run(Func<ETask> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            await SwitchToThreadPool();
            await func.Invoke();
        }

        /// <summary>
        /// Run func and return result on thread pool.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="func">func</param>
        /// <returns>Task with result</returns>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static async ETask<T> Run<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            await SwitchToThreadPool();
            return func.Invoke();
        }

        /// <summary>
        /// Run task with result and return result on thread pool.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="func">func</param>
        /// <returns>Task with result</returns>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static async ETask<T> Run<T>(Func<ETask<T>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            await SwitchToThreadPool();
            return await func.Invoke();
        }
    }
}
