using System;
using System.Threading.Tasks.Sources;

namespace EasyTask
{
    internal interface IETaskSource : IValueTaskSource
    {
        new ETaskStatus GetStatus(short token);
        new void GetResult(short token);
        void OnCompleted(Action<object> continuation, object state, short token);
        

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
            => (ValueTaskSourceStatus)(int)GetStatus(token);
        void IValueTaskSource.GetResult(short token) => GetResult(token);

        void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
            => OnCompleted(continuation, state, token/*, ignore flag */);
    }
}
