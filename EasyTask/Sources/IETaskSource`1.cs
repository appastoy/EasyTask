using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace EasyTask
{
    internal interface IETaskSource<T> : IETaskSource, IValueTaskSource<T>
    {
        new T GetResult(short token);
        new ETaskStatus GetStatus(short token);
        void IETaskSource.GetResult(short token) => GetResult(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token)
            => (ValueTaskSourceStatus)(int)((IETaskSource)this).GetStatus(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T IValueTaskSource<T>.GetResult(short token) => GetResult(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IValueTaskSource<T>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
            => OnCompleted(continuation, state, token/*, ignore flag */);
    }
}
