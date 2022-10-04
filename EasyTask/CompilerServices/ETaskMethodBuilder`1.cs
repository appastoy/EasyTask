using System;
using System.Diagnostics;
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

        public ETask<T> Task
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => exception != null ? ETask.FromException<T>(exception) :
                   promise != null ? promise.Task :
                   ETask.FromResult(result);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETaskMethodBuilder<T> Create() => default;

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            if (promise is null)
                this.exception = exception;
            else
                promise.TrySetException(exception);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T result)
        {
            if (promise != null)
                promise.TrySetResult(result);
            else
                this.result = result;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (promise == null)
                MoveNextPromise<TStateMachine, T>.Create(ref stateMachine, out promise);
            awaiter.OnCompleted(promise.InvokeMoveNext);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, 
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (promise == null)
                MoveNextPromise<TStateMachine, T>.Create(ref stateMachine, out promise);
            awaiter.UnsafeOnCompleted(promise.InvokeMoveNext);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStateMachine(IAsyncStateMachine _)
        {
            
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) 
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();
    }
}
