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
        {
            var promise = WhenAllPromise.Create(tasks);
            return new ETask(promise, promise.Token);
        }

        public static ETask WhenAll(IEnumerable<ETask> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var promise = WhenAllPromise.Create(tasks.ToReadOnlyList());
            return new ETask(promise, promise.Token);
        }

        internal sealed class WhenAllPromise : WhenPromise<WhenAllPromise>
        {
            int completedCount;

            protected override bool CheckCompleted()
                => Interlocked.Increment(ref completedCount) == TaskCount;
        }
    }
}
