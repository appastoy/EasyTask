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

    internal sealed class MoveNextRunner<TStateMachine> : ETaskCompletionSourceGeneric<MoveNextRunner<TStateMachine>>, IMoveNextRunner
        where TStateMachine : IAsyncStateMachine
    {
        TStateMachine stateMachine;
        public Action InvokeMoveNext { get; }

        public static MoveNextRunner<TStateMachine> Create(ref TStateMachine stateMachine)
        {
            var promise = Rent();
            promise.stateMachine = stateMachine;
            return promise;
        }
        public MoveNextRunner() => InvokeMoveNext = MoveNext;
        void MoveNext() => stateMachine.MoveNext();
        protected override void Reset()
        {
            base.Reset();
            stateMachine = default;
        }
    }
}