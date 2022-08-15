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
        {
            var promise = WhenAnyPromise.Create(tasks);
            return new ETask(promise, promise.Token);
        }

        public static ETask WhenAny(IEnumerable<ETask> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var promise = WhenAnyPromise.Create(tasks.ToReadOnlyList());
            return new ETask(promise, promise.Token);
        }

        internal sealed class WhenAnyPromise : WhenPromise<WhenAnyPromise>
        {
            int completedCount;

            protected override bool CheckCompleted()
                => Interlocked.Increment(ref completedCount) == 1;
        }
    }
}
