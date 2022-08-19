#pragma warning disable CS8604

namespace EasyTask
{
    internal interface IETaskCompletionSource<T> : IETaskCompletionSource, IETaskSource<T>
    {
        new ETask<T> Task { get; }
        void TrySetResult(T result);
        ETask IETaskCompletionSource.Task => new ETask(this, Token);
        void IETaskCompletionSource.TrySetResult() => TrySetResult(default);
    }
}
