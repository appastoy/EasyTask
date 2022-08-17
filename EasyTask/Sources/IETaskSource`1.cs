using System;
using System.Threading.Tasks.Sources;

namespace EasyTask
{
    internal interface IETaskSource<T> : IETaskSource, IValueTaskSource<T>
    {
        new T GetResult(short token);
        new ETaskStatus GetStatus(short token);
        void IETaskSource.GetResult(short token) => GetResult(token);
        
        ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token)
            => (ValueTaskSourceStatus)(int)((IETaskSource)this).GetStatus(token);

        T IValueTaskSource<T>.GetResult(short token) => GetResult(token);

        void IValueTaskSource<T>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
            => OnCompleted(continuation, state, token/*, ignore flag */);
    }

    internal interface IETaskCompletionSource<T> : IETaskCompletionSource, IETaskSource<T>
    {
        new ETask<T> Task { get; }
        ETask IETaskCompletionSource.Task => new ETask(this, Token);
        void TrySetResult(T result);
#pragma warning disable CS8604
        void IETaskCompletionSource.TrySetResult() => TrySetResult(default);
#pragma warning restore CS8604
    }
}
