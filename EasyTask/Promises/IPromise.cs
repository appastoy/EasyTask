using EasyTask.Sources;
using System;

namespace EasyTask.Promises
{
    internal interface IPromise : IETaskSource
    {
        ETask Task { get; }
        short Token { get; }
        void TrySetResult();
        void TrySetException(Exception exception);
        void TrySetCanceled(OperationCanceledException operationCanceledException);
    }

    internal interface IPromise<T> : IPromise, IETaskSource<T>
    {
        new ETask<T> Task { get; }
        void TrySetResult(T result);
    }
}
