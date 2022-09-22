using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Run action synchronously on current thread..
        /// </summary>
        /// <param name="action">action</param>
        /// <exception cref="ArgumentNullException">action is null</exception>
        public static void RunSynchronously(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using var scope = ETaskSynchronizationContext.CreateScope();

            action.Invoke();
            scope.Current.ProcessPostsLoop();
        }

        /// <summary>
        /// Run func synchronously on current thread..
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="func">func</param>
        /// <returns>Result</returns>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static TResult RunSynchronously<TResult>(Func<TResult> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var scope = ETaskSynchronizationContext.CreateScope();

            var result = func.Invoke();
            scope.Current.ProcessPostsLoop();
            return result;
        }

        /// <summary>
        /// Run ETaskVoid func synchronously on current thread..
        /// </summary>
        /// <param name="func">ETaskVoid func</param>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static void RunVoidSynchronously(Func<ETaskVoid> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var scope = ETaskSynchronizationContext.CreateScope();

            func.Invoke().Forget();
            scope.Current.ProcessPostsLoop();
        }

        /// <summary>
        /// Run ETask func synchronously on current thread..
        /// </summary>
        /// <param name="func">ETask func</param>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static void RunSynchronously(Func<ETask> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            awaiter.GetResult();
        }

        /// <summary>
        /// Run ETask<typeparamref name="TResult"/> func synchronously on current thread..
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="func">ETask<typeparamref name="TResult"/> func</param>
        /// <returns>Result</returns>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static TResult RunSynchronously<TResult>(Func<ETask<TResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            return awaiter.GetResult();
        }

        /// <summary>
        /// Run Task func synchronously on current thread.
        /// </summary>
        /// <param name="func">Task func</param>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static void RunTaskSynchronously(Func<Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            awaiter.GetResult();
        }
        
        /// <summary>
        /// Run Task<typeparamref name="TResult"/> func synchronously on current thread.
        /// </summary>
        /// <typeparam name="TResult">Task result type</typeparam>
        /// <param name="func">Task func</param>
        /// <returns>Result</returns>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static TResult RunTaskSynchronously<TResult>(Func<Task<TResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            
            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            return awaiter.GetResult();
        }

        /// <summary>
        /// Run ValudTask func synchronously on current thread.
        /// </summary>
        /// <param name="func">ValueTask func</param>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static void RunValueTaskSynchronously(Func<ValueTask> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            awaiter.GetResult();
        }

        /// <summary>
        /// Run ValueTask<typeparamref name="TResult"/> func synchronously on current thread.
        /// </summary>
        /// <typeparam name="TResult">ValueTask result type</typeparam>
        /// <param name="func">ValueTask<typeparamref name="TResult"/> func</param>
        /// <returns>Result</returns>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static TResult RunValueTaskSynchronously<TResult>(Func<ValueTask<TResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            return awaiter.GetResult();
        }
    }
}
