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

        internal sealed class ContinuePromise : ETaskCompletionSourceGeneric<ContinuePromise>
        {
            static readonly Action<object> InvokeOnPrevTaskCompleted = OnPrevTaskCompleted;

            public static ContinuePromise Create(in ETask prevTask, Action<ETask> continuation, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.cancellationToken = cancellationToken;
                prevTask.source.OnCompleted(InvokeOnPrevTaskCompleted, promise, prevTask.token);

                return promise;
            }

            ETask prevTask;
            Action<ETask> continuation;
            CancellationToken cancellationToken;

            static void OnPrevTaskCompleted(object obj) 
                => ((ContinuePromise)obj).InvokeContinuationWith();

            void InvokeContinuationWith()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(new OperationCanceledException(cancellationToken));
                    return;
                }

                try
                {
                    continuation.Invoke(prevTask);
                    TrySetResult();
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                }
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
                cancellationToken = default;
            }
        }

        internal sealed class ContinuePromise<T> : ETaskCompletionSourceGeneric<ContinuePromise<T>, T>
        {
            static readonly Action<object> InvokeOnPrevTaskCompleted = OnPrevTaskCompleted;

            public static ContinuePromise<T> Create(in ETask prevTask, Func<ETask, T> continuation, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.cancellationToken = cancellationToken;
                prevTask.source.OnCompleted(InvokeOnPrevTaskCompleted, promise, prevTask.token);

                return promise;
            }

            ETask prevTask;
            Func<ETask, T> continuation;
            CancellationToken cancellationToken;

            static void OnPrevTaskCompleted(object obj)
                => ((ContinuePromise<T>)obj).InvokeContinuationWith();

            void InvokeContinuationWith()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(new OperationCanceledException(cancellationToken));
                    return;
                }

                try
                {
                    var result = continuation.Invoke(prevTask);
                    TrySetResult(result);
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                }
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
                cancellationToken = default;
            }
        }

        internal sealed class ContinueWithStatePromise : ETaskCompletionSourceGeneric<ContinueWithStatePromise>
        {
            static readonly Action<object> InvokeOnPrevTaskCompleted = OnPrevTaskCompleted;

            public static ContinueWithStatePromise Create(in ETask prevTask, Action<ETask, object> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.state = state;
                promise.cancellationToken = cancellationToken;
                prevTask.source.OnCompleted(InvokeOnPrevTaskCompleted, promise, prevTask.token);

                return promise;
            }

            ETask prevTask;
            Action<ETask, object> continuation;
            object state;
            CancellationToken cancellationToken;

            static void OnPrevTaskCompleted(object obj)
                => ((ContinueWithStatePromise)obj).InvokeContinuationWith();

            void InvokeContinuationWith()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(new OperationCanceledException(cancellationToken));
                    return;
                }

                try
                {
                    continuation.Invoke(prevTask, state);
                    TrySetResult();
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                }
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
                state = null;
                cancellationToken = default;
            }
        }

        internal sealed class ContinueWithStatePromise<T> : ETaskCompletionSourceGeneric<ContinueWithStatePromise<T>, T>
        {
            static readonly Action<object> InvokeOnPrevTaskCompleted = OnPrevTaskCompleted;

            public static ContinueWithStatePromise<T> Create(in ETask prevTask, Func<ETask, object, T> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.state = state;
                promise.cancellationToken = cancellationToken;
                prevTask.source.OnCompleted(InvokeOnPrevTaskCompleted, promise, prevTask.token);

                return promise;
            }

            ETask prevTask;
            Func<ETask, object, T> continuation;
            object state;
            CancellationToken cancellationToken;

            static void OnPrevTaskCompleted(object obj)
                => ((ContinueWithStatePromise<T>)obj).InvokeContinuationWith();

            void InvokeContinuationWith()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(new OperationCanceledException(cancellationToken));
                    return;
                }

                try
                {
                    var result = continuation.Invoke(prevTask, state);
                    TrySetResult(result);
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                }
            }

            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
                state = null;
                cancellationToken = default;
            }
        }
    }
}
