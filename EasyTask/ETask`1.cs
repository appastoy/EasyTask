using EasyTask.CompilerServices;
using EasyTask.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8603
#pragma warning disable CS8625
#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask
{
    [StructLayout(LayoutKind.Auto)]
    [AsyncMethodBuilder(typeof(ETaskMethodBuilder<>))]
    public readonly partial struct ETask<T>
    {
        readonly IETaskSource<T> source;
        readonly short token;
        readonly T result;

        public ETaskStatus Status => source?.GetStatus(token) ?? ETaskStatus.Succeeded;
        public bool IsCompleted => Status != ETaskStatus.Pending;
        public bool IsCompletedSuccessfully => Status == ETaskStatus.Succeeded;
        public bool IsFaulted => Status == ETaskStatus.Faulted;
        public bool IsCanceled => Status == ETaskStatus.Canceled;


        public T Result => source != null ? source.GetResult(token) : default;


        public Exception? Exception => 
            source is ETask.ExceptionSource exceptionSource ? exceptionSource.GetException() :
            source is ETask.CanceledSource canceledSource ? canceledSource.Exception : 
            null;


        internal ETask(IETaskSource<T> source, short token)
        {
            this.source = source;
            this.token = token;
            result = default;
        }

        internal ETask(T result)
        {
            source = default;
            token = default;
            this.result = result;
        }

        public Awaiter GetAwaiter() => new Awaiter(in this);

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            readonly ETask<T> task;

            public bool IsCompleted => task.IsCompleted;

            internal Awaiter(in ETask<T> task) => this.task = task;

            public T GetResult() => task.source is null ? task.result : task.source.GetResult(task.token);

            public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation)
            {
                if (task.source is null)
                    continuation.Invoke();
                else
                    OnCompleted(DelegateCache.InvokeAsActionObject, continuation);
            }
            internal void OnCompleted(Action<object> continuation, object state)
            {
                if (task.source is null)
                    continuation.Invoke(state);
                else
                    task.source.OnCompleted(continuation, state, task.token);
            }
        }
    }
}