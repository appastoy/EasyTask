#pragma warning disable CS8604

using System.Runtime.CompilerServices;

namespace EasyTask
{
    internal interface IETaskCompletionSource<T> : IETaskCompletionSource, IETaskSource<T>
    {
        new ETask<T> Task { get; }
        void TrySetResult(T result);

        ETask IETaskCompletionSource.Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(this, Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IETaskCompletionSource.TrySetResult() => TrySetResult(default);
    }
}
