using EasyTask.CompilerServices;
using EasyTask.Sources;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EasyTask
{
    [StructLayout(LayoutKind.Auto, Pack = 2)]
    [AsyncMethodBuilder(typeof(ETaskMethodBuilder))]
    public readonly partial struct ETask
    {
        readonly IETaskSource source;
        readonly short token;

        public ETaskStatus Status => source?.GetStatus(token) ?? ETaskStatus.Succeeded;
        public bool IsCompleted => Status != ETaskStatus.Pending;
        public bool IsCompletedSuccessfully => Status == ETaskStatus.Succeeded;
        public bool IsFault => Status == ETaskStatus.Faulted;
        public bool IsCanceled => Status == ETaskStatus.Canceled;

        public Exception? Exception => source is ExceptionSource exceptionSource ? exceptionSource.GetException() : null;

        internal ETask(IETaskSource source, short token)
        {
            this.source = source;
            this.token = token;
        }
    }
}
