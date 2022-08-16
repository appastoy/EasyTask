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

        internal sealed class WhenAllPromise : WhenPromise<WhenAllPromise>
        {
            int completedCount;

            protected override bool CheckCompleted()
                => Interlocked.Increment(ref completedCount) == TaskCount;

            protected override void BeforeReturn()
            {
                completedCount = 0;
                base.BeforeReturn();
            }
        }
    }
}
