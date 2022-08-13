using System;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static readonly ETask CompletedTask = new ETask();
        public static readonly ETask CanceledTask = new ETask(
            new CanceledSource(new OperationCanceledException(CancellationToken.None)), 0);
    }
}
