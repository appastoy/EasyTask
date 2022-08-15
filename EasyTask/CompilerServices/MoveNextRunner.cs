using EasyTask.Promises;
using System;
using System.Runtime.CompilerServices;

namespace EasyTask.CompilerServices
{
    internal interface IMoveNextRunner
    {
        Action InvokeMoveNext { get; }
        void Return();
    }

    internal sealed class MoveNextRunner<TStateMachine> : Promise<MoveNextRunner<TStateMachine>>, IMoveNextRunner
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

#pragma warning disable CS8618
        public MoveNextRunner() => InvokeMoveNext = MoveNext;
#pragma warning restore CS8618

        void MoveNext() => stateMachine.MoveNext();

        protected override void BeforeReturn()
        {
#pragma warning disable CS8601
            stateMachine = default;
#pragma warning restore CS8601
        }
    }
}
