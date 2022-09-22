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
        /// Await until all tasks are completed.
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask WhenAll(params ETask[] tasks)
        {
            if (tasks.Length == 0)
                return CompletedTask;
            
            return WhenAllPromise.Create(tasks).Task;
        }

        /// <summary>
        /// Await until all tasks are completed.
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">tasks is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask WhenAll(IEnumerable<ETask> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var taskList = tasks.ToReadOnlyList();
            if (taskList.Count == 0)
                return CompletedTask;

            return WhenAllPromise.Create(taskList).Task;
        }

        /// <summary>
        /// Await until all tasks are completed.
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>Task with results of all tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask<T[]> WhenAll<T>(params ETask<T>[] tasks)
        {
            if (tasks.Length == 0)
                return FromResult(Array.Empty<T>());
            
            return WhenAllPromise<T>.Create(tasks).Task;
        }
        
        /// <summary>
        /// Await until all tasks are completed.
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <param name="tasks">Tasks</param>
        /// <returns>Task with results of all tasks.</returns>
        /// <exception cref="ArgumentNullException">tasks is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask<T[]> WhenAll<T>(IEnumerable<ETask<T>> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var taskList = tasks.ToReadOnlyList();
            if (taskList.Count == 0)
                return FromResult(Array.Empty<T>());

            return WhenAllPromise<T>.Create(taskList).Task;
        }

        internal sealed class WhenAllPromise : WhenPromise<WhenAllPromise>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override bool CheckCompleted()
                => Interlocked.Increment(ref countCompleted) == TaskCount;
        }

        internal sealed class WhenAllPromise<T> : WhenPromise<WhenAllPromise<T>, T, T[]>
        {
            T[]? results;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void OnTaskCompleted(in ETask<T>.Awaiter awaiter, int index)
            {
                if (results == null)
                    results = new T[TaskCount];
                try
                {
                    results[index] = awaiter.GetResult();
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                    return;
                }

                if (Interlocked.Increment(ref countCompleted) == TaskCount)
                    TrySetResult(results);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Reset()
            {
                base.Reset();
                results = default;
            }
        }
    }
}
