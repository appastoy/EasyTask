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

        internal sealed class WhenAnyPromise : WhenPromise<WhenAnyPromise>
        {
            int completedCount;

            protected override bool CheckCompleted()
                => Interlocked.Increment(ref completedCount) == 1;

            protected override void BeforeReturn()
            {
                completedCount = 0;
                base.BeforeReturn();
            }
        }
    }
}
