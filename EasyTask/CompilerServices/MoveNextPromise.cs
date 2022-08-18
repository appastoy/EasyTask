﻿using EasyTask.Sources;
using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask.CompilerServices
{

    internal interface IMoveNextPromise : IETaskCompletionSource, IMoveNextRunner
    {

    }

    internal sealed class MoveNextPromise<TStateMachine> : ETaskCompletionSourceGeneric<MoveNextPromise<TStateMachine>>, IMoveNextPromise
        where TStateMachine : IAsyncStateMachine
    {
        TStateMachine stateMachine;
        public Action InvokeMoveNext { get; }

        public static MoveNextPromise<TStateMachine> Create(ref TStateMachine stateMachine)
        {
            var promise = Rent();
            promise.stateMachine = stateMachine;
            return promise;
        }
        public MoveNextPromise() => InvokeMoveNext = MoveNext;
        void MoveNext() => stateMachine.MoveNext();
        protected override void Reset()
        {
            base.Reset();
            stateMachine = default;
        }
    }
}