using EasyTask.CompilerServices;
using EasyTask.Helpers;
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
        public static readonly ETask CompletedTask = new ETask();

        readonly IETaskSource source;
        readonly short token;

        public ETaskStatus Status => source?.GetStatus(token) ?? ETaskStatus.Succeeded;
        public bool IsCompleted => Status != ETaskStatus.Pending;
        public bool IsCompletedSuccessfully => Status == ETaskStatus.Succeeded;
        public bool IsFaulted => Status == ETaskStatus.Faulted;
        public bool IsCanceled => Status == ETaskStatus.Canceled;

        public Exception? Exception => 
            source is ExceptionSource exceptionSource ? exceptionSource.GetException() :
            source is CanceledSource canceledSource ? canceledSource.Exception : 
            null;

        internal ETask(IETaskSource source, short token)
        {
            this.source = source;
            this.token = token;
        }

        public Awaiter GetAwaiter()
        {
            return new Awaiter(in this);
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            readonly ETask task;

            public bool IsCompleted => task.IsCompleted;

            internal Awaiter(in ETask task)
            {
                this.task = task;
            }

            public void GetResult()
            {
                task.source?.GetResult(task.token);
            }

            public void OnCompleted(Action continuation)
                => UnsafeOnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation)
            {
                if (task.source is null)
                    continuation?.Invoke();
                else
                    OnCompleted(DelegateCache.InvokeAsActionObject, continuation);
            }
            internal void OnCompleted(Action<object> continuation, object state)
            {
                if (task.source is null)
                    continuation?.Invoke(state);
                else
                    task.source?.OnCompleted(continuation, state, task.token);
            }
        }
    }
}
