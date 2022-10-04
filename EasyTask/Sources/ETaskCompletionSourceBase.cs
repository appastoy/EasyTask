using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EasyTask.Sources
{
    public abstract class ETaskCompletionSourceBase<TSource>
        : ETaskSourceBase<TSource>, IETaskCompletionSource
        where TSource : ETaskCompletionSourceBase<TSource>, new()
    {
        public ETask Task
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(this, Token);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult(short token)
        {
            try 
            {
                if (Token != token)
                    throw new InvalidOperationException("You can only call one of these methods once. (ETask.Awaiter.GetResult() or await ETask)");
                GetResultInternal(token); 
            }
            finally 
            {
                Return();
            }
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrySetResult()
        {
            if (IsCompletedFirst())
                InvokeContinuation();
        }
    }
}
