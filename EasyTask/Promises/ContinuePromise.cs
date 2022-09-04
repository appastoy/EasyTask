using EasyTask.Sources;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask.Promises
{
    internal interface IContinuePromise
    {
        CancellationToken CancellationToken { get; }
        void OnTrySetResult();
        void TrySetCanceled(OperationCanceledException exception);
        void TrySetException(Exception exception);
    }

    internal abstract class ContinuePromiseBase<T> : ETaskCompletionSourceBase<T>, IContinuePromise
        where T : ContinuePromiseBase<T>, new()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Create(IETaskSource source, short token, in CancellationToken cancellationToken)
        {
            var promise = Rent();
            promise.CancellationToken = cancellationToken;
            source.OnCompleted(ContinuePromiseDelegateCache.InvokeOnPrevTaskCompleted, promise, token);

            return promise;
        }

        public CancellationToken CancellationToken { get; private set; }

        public abstract void OnTrySetResult();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Reset()
        {
            base.Reset();
            CancellationToken = default;
        }
    }

    internal static class ContinuePromiseDelegateCache
    {
        public static readonly Action<object> InvokeOnPrevTaskCompleted = OnPrevTaskCompleted;

        static void OnPrevTaskCompleted(object obj)
        {
            var promise = (IContinuePromise)obj;
            var cancelToken = promise.CancellationToken;
            if (cancelToken.IsCancellationRequested)
            {
                promise.TrySetCanceled(new OperationCanceledException(cancelToken));
                return;
            }

            try
            {
                promise.OnTrySetResult();
            }
            catch (Exception exception)
            {
                promise.TrySetException(exception);
            }
        }
    }
}
