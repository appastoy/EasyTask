using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace EasyTask.CompilerServices
{
    [StructLayout(LayoutKind.Auto)]
    public struct ETaskVoidMethodBuilder
    {
        IMoveNextRunner? promise;

        public ETaskVoid Task => default;

        public static ETaskVoidMethodBuilder Create() => default;

        public void SetException(Exception exception)
        {
            try
            {
                promise?.Return();
            }
            finally
            {
                ETask.PublishUnhandledException(ExceptionDispatchInfo.Capture(exception));
            }
        }

        public void SetResult()
        {
            promise?.Return();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            promise ??= MoveNextRunner<TStateMachine>.Create(ref stateMachine);
            awaiter.OnCompleted(promise.InvokeMoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, 
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            promise ??= MoveNextRunner<TStateMachine>.Create(ref stateMachine);
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
