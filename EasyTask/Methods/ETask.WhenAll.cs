using EasyTask.Helpers;
using EasyTask.Promises;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static ETask WhenAll(params ETask[] tasks)
            => WhenAllPromise.Create(tasks).Task;

        public static ETask WhenAll(IEnumerable<ETask> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            return WhenAllPromise.Create(tasks.ToReadOnlyList()).Task;
        }

        public static ETask<T[]> WhenAll<T>(params ETask<T>[] tasks)
            => WhenAllPromise<T>.Create(tasks).Task;

        public static ETask<T[]> WhenAll<T>(IEnumerable<ETask<T>> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            return WhenAllPromise<T>.Create(tasks.ToReadOnlyList()).Task;
        }

        internal sealed class WhenAllPromise : WhenPromise<WhenAllPromise>
        {
            protected override bool CheckCompleted()
                => Interlocked.Increment(ref countCompleted) == taskCount;
        }

        internal sealed class WhenAllPromise<T> : WhenPromise<WhenAllPromise<T>, T, T[]>
        {
            T[]? results;

            protected override void OnTaskCompleted(in ETask<T>.Awaiter awaiter, int index)
            {
                if (results == null)
                    results = new T[taskCount];
                try
                {
                    results[index] = awaiter.GetResult();
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                    return;
                }

                if (Interlocked.Increment(ref countCompleted) == taskCount)
                    TrySetResult(results);
            }

            protected override void BeforeReturn()
            {
                results = default;
                base.BeforeReturn();
            }
        }
    }
}
