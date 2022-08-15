using EasyTask.CompilerServices;
using EasyTask.Helpers;
using EasyTask.Sources;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EasyTask
{
    [StructLayout(LayoutKind.Auto, Pack = 2)]
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

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
        public T Result => source != null ? source.GetResult(token) : default;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.

        public Exception? Exception => 
            source is ETask.ExceptionSource exceptionSource ? exceptionSource.GetException() :
            source is ETask.CanceledSource canceledSource ? canceledSource.Exception : 
            null;

#pragma warning disable CS8625 // Null 리터럴을 null을 허용하지 않는 참조 형식으로 변환할 수 없습니다.
#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
        internal ETask(IETaskSource<T> source, short token)

        {
            this.source = source;
            this.token = token;

#pragma warning disable CS8601 // 가능한 null 참조 할당입니다.
            result = default;
#pragma warning restore CS8601 // 가능한 null 참조 할당입니다.
        }

        internal ETask(T result)
        {

            source = default;
            token = default;
            this.result = result;
        }
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
#pragma warning restore CS8625 // Null 리터럴을 null을 허용하지 않는 참조 형식으로 변환할 수 없습니다.

        public Awaiter GetAwaiter()
        {
            return new Awaiter(in this);
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            readonly ETask<T> task;

            public bool IsCompleted => task.IsCompleted;

            internal Awaiter(in ETask<T> task)
            {
                this.task = task;
            }

            public T GetResult()
            {
                if (task.source != null)
                    return task.source.GetResult(task.token);
                else
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                    return default;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
            }

            public void OnCompleted(Action continuation)
                => UnsafeOnCompleted(continuation);

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
