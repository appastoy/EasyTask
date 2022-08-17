#pragma warning disable CS8604

namespace EasyTask
{
    internal interface IETaskCompletionSource<T> : IETaskCompletionSource, IETaskSource<T>
    {
        new ETask<T> Task { get; }
        ETask IETaskCompletionSource.Task => new ETask(this, Token);
        void TrySetResult(T result);
        void IETaskCompletionSource.TrySetResult() => TrySetResult(default);
    }
}
