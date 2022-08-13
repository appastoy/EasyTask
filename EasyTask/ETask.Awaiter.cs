using System;
using System.Runtime.CompilerServices;

namespace EasyTask
{
    public readonly partial struct ETask
    {
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
                    task.source.OnCompleted(DelegateCache.InvokeAsActionObject, continuation, task.token);
            }
        }
    }

    
}
