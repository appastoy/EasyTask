using EasyTask.Promises;
using EasyTask.Sources;
using System;
using System.Threading;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace EasyTask
{
    partial struct ETask
    {
        public ETask ContinueWith(Action<ETask> continuation, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    continuation.Invoke(this);
                    return CompletedTask;
                }
                catch (Exception exception)
                {
                    return FromException(exception);
                }
            }

            return ContinuePromise.Create(in this, continuation, in cancellationToken).Task;
        }

        public ETask<T> ContinueWith<T>(Func<ETask, T> continuation, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    return FromResult(continuation.Invoke(this));
                }
                catch (Exception exception)
                {
                    return FromException<T>(exception);
                }
            }

            return ContinuePromise<T>.Create(in this, continuation, in cancellationToken).Task;
        }

        public ETask ContinueWith(Action<ETask, object> continuation, object state, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    continuation.Invoke(this, state);
                    return CompletedTask;
                }
                catch (Exception exception)
                {
                    return FromException(exception);
                }
            }

            return ContinueWithStatePromise.Create(in this, continuation, state, in cancellationToken).Task;
        }

        public ETask<T> ContinueWith<T>(Func<ETask, object, T> continuation, object state, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    return FromResult(continuation.Invoke(this, state));
                }
                catch (Exception exception)
                {
                    return FromException<T>(exception);
                }
            }

            return ContinueWithStatePromise<T>.Create(in this, continuation, state, in cancellationToken).Task;
        }

        internal sealed class ContinuePromise : ContinuePromiseBase<ContinuePromise>
        {
            public static ContinuePromise Create(in ETask prevTask, Action<ETask> continuation, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.Initialize(in prevTask, in cancellationToken);
                promise.continuation = continuation;
                prevTask.source.OnCompleted(
                    ContinuePromiseDelegateCache.InvokeOnPrevTaskCompleted
                    , promise
                    , prevTask.token);

                return promise;
            }

            Action<ETask> continuation;

            protected override void OnTrySetResult()
            {
                continuation.Invoke(prevTask);
                TrySetResult();
            }

            protected override void Reset()
            {
                base.Reset();
                continuation = null;
            }
        }

        internal sealed class ContinueWithStatePromise : ContinuePromiseBase<ContinueWithStatePromise>
        {
            public static ContinueWithStatePromise Create(in ETask prevTask, Action<ETask, object> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.Initialize(in prevTask, in cancellationToken);
                promise.continuation = continuation;
                promise.state = state;
                prevTask.source.OnCompleted(
                    ContinuePromiseDelegateCache.InvokeOnPrevTaskCompleted
                    , promise
                    , prevTask.token);

                return promise;
            }

            Action<ETask, object> continuation;
            object state;

            protected override void OnTrySetResult()
            {
                continuation.Invoke(prevTask, state);
                TrySetResult();
            }

            protected override void Reset()
            {
                base.Reset();
                continuation = null;
                state = null;
            }
        }

        internal sealed class ContinuePromise<T> : ContinueWithStatePromiseBase<ContinuePromise<T>, T>
        {

            public static ContinuePromise<T> Create(in ETask prevTask, Func<ETask, T> continuation, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.Initialize(in prevTask, in cancellationToken);
                promise.continuation = continuation;
                prevTask.source.OnCompleted(
                    ContinuePromiseDelegateCache.InvokeOnPrevTaskCompleted
                    , promise
                    , prevTask.token);

                return promise;
            }

            Func<ETask, T> continuation;


            protected override void OnTrySetResult()
            {
                var result = continuation.Invoke(prevTask);
                TrySetResult(result);
            }

            protected override void Reset()
            {
                base.Reset();
                continuation = null;
            }
        }

        

        internal sealed class ContinueWithStatePromise<T> : ContinueWithStatePromiseBase<ContinueWithStatePromise<T>, T>
        {
            public static ContinueWithStatePromise<T> Create(in ETask prevTask, Func<ETask, object, T> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.Initialize(in prevTask, in cancellationToken);
                promise.continuation = continuation;
                promise.state = state;
                prevTask.source.OnCompleted(ContinuePromiseDelegateCache.InvokeOnPrevTaskCompleted, promise, prevTask.token);

                return promise;
            }

            Func<ETask, object, T> continuation;
            object state;

            protected override void OnTrySetResult()
            {
                var result = continuation.Invoke(prevTask, state);
                TrySetResult(result);
            }

            protected override void Reset()
            {
                base.Reset();
                continuation = null;
                state = null;
            }
        }
    }
}
