using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask.Sources
{
    public abstract class ETaskCompletionSourceBase<TSource, T>
        : ETaskSourceBase<TSource>, IETaskCompletionSource<T>
        where TSource : ETaskCompletionSourceBase<TSource, T>, new()
    {

        T result;

        public ETask<T> Task
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(this, Token);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult(short token)
        {
            try
            {
                if (Token != token)
                    throw new InvalidOperationException("You can only call one of these methods once. (ETask<T>.Result or ETask<T>.Awaiter.GetResult() or await ETask<T>)");
                GetResultInternal(token);
                return result;
            }
            finally
            {
                Return();
            }
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrySetResult(T result)
        {
            if (IsCompletedFirst())
            {
                this.result = result;
                InvokeContinuation();
            }
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Reset()
        {
            base.Reset();
            result = default;
        }
    }
}
