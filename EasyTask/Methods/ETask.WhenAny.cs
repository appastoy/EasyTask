using EasyTask.Helpers;
using EasyTask.Promises;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Await until any task is completed.
        /// </summary>
        /// <param name="tasks">Task</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask WhenAny(params ETask[] tasks)
        {
            if (tasks.Length == 0)
                return CompletedTask;
            
            return WhenAnyPromise.Create(tasks).Task;
        }

        /// <summary>
        /// Await until any task is completed.
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">tasks is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask WhenAny(IEnumerable<ETask> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var taskList = tasks.ToReadOnlyList();
            if (taskList.Count == 0)
                return CompletedTask;

            return WhenAnyPromise.Create(taskList).Task;
        }

        /// <summary>
        /// Await until any task is completed.
        /// </summary>
        /// <typeparam name="T">ETask result type.</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>First completed task and index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask<(T, int)>  WhenAny<T>(params ETask<T>[] tasks)
        {
            if (tasks.Length == 0)
                return FromResult<(T, int)>(default);
            
            return WhenAnyPromise<T>.Create(tasks).Task;
        }

        /// <summary>
        /// Await until any task is completed.
        /// </summary>
        /// <typeparam name="T">ETask result type</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>First completed task and index.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask<(T, int)> WhenAny<T>(IEnumerable<ETask<T>> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            return WhenAnyPromise<T>.Create(tasks.ToReadOnlyList()).Task;
        }

        internal sealed class WhenAnyPromise : WhenPromise<WhenAnyPromise>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override bool CheckCompleted()
                => Interlocked.Increment(ref countCompleted) == 1;
        }

        internal sealed class WhenAnyPromise<T> : WhenPromise<WhenAnyPromise<T>, T, (T, int)>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void OnTaskCompleted(in ETask<T>.Awaiter awaiter, int index)
            {
                T result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                    return;
                }

                if (Interlocked.Increment(ref countCompleted) == 1)
                    TrySetResult((result, index));
            }
        }
    }
}
