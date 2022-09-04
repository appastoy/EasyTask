using EasyTask.Sources;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask.CompilerServices
{

    internal interface IMoveNextPromise : IETaskCompletionSource, IMoveNextRunner
    {

    }

    internal sealed class MoveNextPromise<TStateMachine> : ETaskCompletionSourceBase<MoveNextPromise<TStateMachine>>, IMoveNextPromise
        where TStateMachine : IAsyncStateMachine
    {
        TStateMachine stateMachine;
        public Action InvokeMoveNext { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Create(ref TStateMachine stateMachine, out IMoveNextPromise outPromise)
        {
            var promise = Rent();
            outPromise = promise;
            promise.stateMachine = stateMachine;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveNextPromise() => InvokeMoveNext = MoveNext;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MoveNext() => stateMachine.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Reset()
        {
            base.Reset();
            stateMachine = default;
        }
    }
}