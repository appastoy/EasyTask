using EasyTask.Sources;
using System;
using System.Threading;

namespace EasyTask.Promises
{
    internal interface IPromise : IETaskSource
    {
        short Token { get; }
        void TrySetResult();
        void TrySetException(Exception exception);
        void TrySetCanceled(CancellationToken cancellationToken = default);
    }
}
