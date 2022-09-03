using EasyTask.Sources;
using System.Threading;

namespace EasyTask.Promises
{
    internal abstract class ContinuePromiseBase<TPromise, TResult> 
        : ETaskCompletionSourceBase<TPromise, TResult>, IContinuePromise
        where TPromise : ContinuePromiseBase<TPromise, TResult>, new()
    {
        public static TPromise Create(IETaskSource source, short token, in CancellationToken cancellationToken)
        {
            var promise = Rent();
            promise.CancellationToken = cancellationToken;
            source.OnCompleted(ContinuePromiseDelegateCache.InvokeOnPrevTaskCompleted, promise, token);

            return promise;
        }

        public CancellationToken CancellationToken { get; private set; }

        public abstract void OnTrySetResult();

        protected override void Reset()
        {
            base.Reset();
            CancellationToken = default;
        }
    }
}
