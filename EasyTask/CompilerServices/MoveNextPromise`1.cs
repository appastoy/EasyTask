using EasyTask.Promises;
using System;
using System.Runtime.CompilerServices;

namespace EasyTask.CompilerServices
{

    internal interface IMoveNextPromise<T> : IPromise<T>, IMoveNextRunner
    {

    }

    internal sealed class MoveNextPromise<TStateMachine, T> : Promise<MoveNextPromise<TStateMachine, T>, T>, IMoveNextPromise<T>
        where TStateMachine : IAsyncStateMachine
    {
        TStateMachine stateMachine;
        public Action InvokeMoveNext { get; }

        public static MoveNextPromise<TStateMachine, T> Create(ref TStateMachine stateMachine)
        {
            var promise = Rent();
            promise.stateMachine = stateMachine;
            return promise;
        }

#pragma warning disable CS8618
        public MoveNextPromise() => InvokeMoveNext = MoveNext;
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
