using EasyTask.Sources;
using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask.CompilerServices
{
    internal interface IMoveNextRunner
    {
        Action InvokeMoveNext { get; }
        void Return();
    }

    internal sealed class MoveNextRunner<TStateMachine> : ETaskCompletionSourceBase<MoveNextRunner<TStateMachine>>, IMoveNextRunner
        where TStateMachine : IAsyncStateMachine
    {
        TStateMachine stateMachine;
        public Action InvokeMoveNext { get; }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Create(ref TStateMachine stateMachine, out IMoveNextRunner outPromise)
        {
            var promise = Rent();
            outPromise = promise;
            promise.stateMachine = stateMachine;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveNextRunner() => InvokeMoveNext = MoveNext;
        
        void MoveNext() => stateMachine.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Reset()
        {
            base.Reset();
            stateMachine = default;
        }
    }
}