using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace EasyTask
{
    internal interface IETaskSource : IValueTaskSource
    {
        new ETaskStatus GetStatus(short token);
        new void GetResult(short token);
        void OnCompleted(Action<object> continuation, object state, short token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
            => (ValueTaskSourceStatus)(int)GetStatus(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IValueTaskSource.GetResult(short token) => GetResult(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
            => OnCompleted(continuation, state, token/*, ignore flag */);
    }
}
