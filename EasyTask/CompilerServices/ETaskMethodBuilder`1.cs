using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EasyTask.CompilerServices
{
    [StructLayout(LayoutKind.Auto)]
    public struct ETaskMethodBuilder<T>
    {
        IMoveNextPromise<T>? promise;
        Exception? exception;
        T result;

        public ETask<T> Task =>
            exception != null ? ETask.FromException<T>(exception) :
            promise != null ? promise.Task :
            ETask.FromResult(result);

        public static ETaskMethodBuilder<T> Create() => default;

        public void SetException(Exception exception)
        {
            if (promise is null)
                this.exception = exception;
            else
                promise.TrySetException(exception);
        }

        public void SetResult(T result)
        {
            if (promise != null)
                promise.TrySetResult(result);
            else
                this.result = result;
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            promise ??= MoveNextPromise<TStateMachine, T>.Create(ref stateMachine);
            awaiter.OnCompleted(promise.InvokeMoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, 
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            promise ??= MoveNextPromise<TStateMachine, T>.Create(ref stateMachine);
            awaiter.UnsafeOnCompleted(promise.InvokeMoveNext);
        }

        public void SetStateMachine(IAsyncStateMachine _)
        {
            
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) 
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();
    }
}
