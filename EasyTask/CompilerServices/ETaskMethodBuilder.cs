using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EasyTask.CompilerServices
{
    [StructLayout(LayoutKind.Auto)]
    public struct ETaskMethodBuilder
    {
        IMoveNextPromise? promise;
        Exception? exception;

        public ETask Task =>
            exception != null ? ETask.FromException(exception) :
            promise != null ? promise.Task :
            ETask.CompletedTask;

        public static ETaskMethodBuilder Create() => default;

        public void SetException(Exception exception)
        {
            if (promise is null)
                this.exception = exception;
            else
                promise.TrySetException(exception);
        }

        public void SetResult()
        {
            promise?.TrySetResult();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            promise ??= MoveNextPromise<TStateMachine>.Create(ref stateMachine);
            awaiter.OnCompleted(promise.InvokeMoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, 
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            promise ??= MoveNextPromise<TStateMachine>.Create(ref stateMachine);
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
