using System;

namespace EasyTask
{
    internal interface IETaskCompletionSource : IETaskSource
    {
        ETask Task { get; }
        short Token { get; }
        void TrySetException(Exception exception);
        void TrySetCanceled(OperationCanceledException exception);
        void TrySetResult();
    }
}
