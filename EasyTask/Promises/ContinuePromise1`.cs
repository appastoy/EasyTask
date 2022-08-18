using EasyTask.Sources;
using System;
using System.Threading;

namespace EasyTask.Promises
{
    internal abstract class ContinueWithStatePromiseBase<TPromise, T> : ETaskCompletionSourceGeneric<TPromise, T>, IContinuePromise
        where TPromise : ContinueWithStatePromiseBase<TPromise, T>, new()
    {
        CancellationToken cancellationToken;
        protected ETask prevTask { get; private set; }

        protected void Initialize(in ETask prevTask, in CancellationToken cancellationToken)
        {
            this.prevTask = prevTask;
            this.cancellationToken = cancellationToken;
        }

        void IContinuePromise.InvokeContinueWith()
        {
            if (cancellationToken.IsCancellationRequested)
            {
                TrySetCanceled(new OperationCanceledException(cancellationToken));
                return;
            }

            try
            {
                OnTrySetResult();
            }
            catch (Exception exception)
            {
                TrySetException(exception);
            }
        }

        protected abstract void OnTrySetResult();

        protected override void Reset()
        {
            base.Reset();
            prevTask = default;
            cancellationToken = default;
        }
    }
}
