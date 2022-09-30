using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Run ETask func synchronously on current thread..
        /// </summary>
        /// <param name="func">ETask func</param>
        /// <exception cref="ArgumentNullException">func is null</exception>
        public static void RunSynchronously(Func<ETask> func, [CallerMemberName]string name = "")
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
