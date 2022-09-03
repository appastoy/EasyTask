using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTask
{
    partial struct ETask
    {
        public static void RunSynchronously(Action action)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            action.Invoke();
            scope.Current.ProcessPostsLoop();
        }

        public static TResult RunSynchronously<TResult>(Func<TResult> func)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            var result = func.Invoke();
            scope.Current.ProcessPostsLoop();
            return result;
        }

        public static void RunVoidSynchronously(Func<ETaskVoid> func)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            func.Invoke().Forget();
            scope.Current.ProcessPostsLoop();
        }

        public static void RunSynchronously(Func<ETask> func)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            awaiter.GetResult();
        }

        public static TResult RunSynchronously<TResult>(Func<ETask<TResult>> func)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            return awaiter.GetResult();
        }

        public static void RunTaskSynchronously(Func<Task> func)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            awaiter.GetResult();
        }

        public static TResult RunTaskSynchronously<TResult>(Func<Task<TResult>> func)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            return awaiter.GetResult();
        }

        public static void RunValueTaskSynchronously(Func<ValueTask> func)
        {
            using var scope = ETaskSynchronizationContext.CreateScope();

            var awaiter = func.Invoke().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                scope.Current.ProcessPosts();
                Thread.Yield();
            }
            awaiter.GetResult();
        }

        public static TResult RunValueTaskSynchronously<TResult>(Func<ValueTask<TResult>> func)
        {
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
