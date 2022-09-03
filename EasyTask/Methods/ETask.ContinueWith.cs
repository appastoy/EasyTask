﻿using EasyTask.Promises;
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

        public ETask<TResult> ContinueWith<TResult>(Func<ETask, TResult> continuation, CancellationToken cancellationToken = default)
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
                    return FromException<TResult>(exception);
                }
            }

            return ContinuePromise<TResult>.Create(in this, continuation, in cancellationToken).Task;
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

        public ETask<TResult> ContinueWith<TResult>(Func<ETask, object, TResult> continuation, object state, CancellationToken cancellationToken = default)
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
                    return FromException<TResult>(exception);
                }
            }

            return ContinueWithStatePromise<TResult>.Create(in this, continuation, state, in cancellationToken).Task;
        }

        internal sealed class ContinuePromise : ContinuePromiseBase<ContinuePromise>
        {
            public static ContinuePromise Create(in ETask prevTask, Action<ETask> continuation, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;

                return promise;
            }

            ETask prevTask;
            Action<ETask> continuation;

            public override void OnTrySetResult()
            {
                continuation.Invoke(prevTask);
                TrySetResult();
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
            }
        }

        internal sealed class ContinueWithStatePromise : ContinuePromiseBase<ContinueWithStatePromise>
        {
            public static ContinueWithStatePromise Create(in ETask prevTask, Action<ETask, object> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.state = state;

                return promise;
            }

            ETask prevTask;
            Action<ETask, object> continuation;
            object state;

            public override void OnTrySetResult()
            {
                continuation.Invoke(prevTask, state);
                TrySetResult();
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
                state = null;
            }
        }

        internal sealed class ContinuePromise<TResult> 
            : ContinuePromiseBase<ContinuePromise<TResult>, TResult>
        {

            public static ContinuePromise<TResult> Create(in ETask prevTask, Func<ETask, TResult> continuation, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;

                return promise;
            }

            ETask prevTask;
            Func<ETask, TResult> continuation;


            public override void OnTrySetResult()
            {
                var result = continuation.Invoke(prevTask);
                TrySetResult(result);
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
            }
        }

        

        internal sealed class ContinueWithStatePromise<TResult> 
            : ContinuePromiseBase<ContinueWithStatePromise<TResult>, TResult>
        {
            public static ContinueWithStatePromise<TResult> Create(in ETask prevTask, Func<ETask, object, TResult> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.state = state;

                return promise;
            }

            ETask prevTask;
            Func<ETask, object, TResult> continuation;
            object state;

            public override void OnTrySetResult()
            {
                var result = continuation.Invoke(prevTask, state);
                TrySetResult(result);
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
                state = null;
            }
        }
    }
}