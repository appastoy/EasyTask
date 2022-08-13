using System;
using System.Threading.Tasks.Sources;

namespace EasyTask.Sources
{
    internal interface IETaskSource : IValueTaskSource
    {
        new ETaskStatus GetStatus(short token);
        void OnCompleted(Action<object?>? continuation, object? state, short token);

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
            => (ValueTaskSourceStatus)(int)GetStatus(token);

        void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
            => OnCompleted(continuation, state, token/*, ignore flag */);
    }
}
