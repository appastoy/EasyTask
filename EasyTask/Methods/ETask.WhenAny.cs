using EasyTask.Helpers;
using EasyTask.Promises;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static ETask WhenAny(params ETask[] tasks)
            => WhenAnyPromise.Create(tasks).Task;

        public static ETask WhenAny(IEnumerable<ETask> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            return WhenAnyPromise.Create(tasks.ToReadOnlyList()).Task;
        }

        public static ETask<(T, int)>  WhenAny<T>(params ETask<T>[] tasks)
            => WhenAnyPromise<T>.Create(tasks).Task;

        public static ETask<(T, int)> WhenAny<T>(IEnumerable<ETask<T>> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            return WhenAnyPromise<T>.Create(tasks.ToReadOnlyList()).Task;
        }

        internal sealed class WhenAnyPromise : WhenPromise<WhenAnyPromise>
        {
            protected override bool CheckCompleted()
                => Interlocked.Increment(ref countCompleted) == 1;
        }

        internal sealed class WhenAnyPromise<T> : WhenPromise<WhenAnyPromise<T>, T, (T, int)>
        {
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
