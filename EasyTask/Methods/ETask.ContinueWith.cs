using EasyTask.Pools;
using EasyTask.Promises;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Set up a delegate that runs continuously after the task completes.
        /// </summary>
        /// <param name="continuation">delegate</param>
        /// <param name="cancellationToken">cancal token</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
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
                finally
                {
                    if (source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
            }

            return ContinuePromise.Create(in this, continuation, in cancellationToken).Task;
        }

        /// <summary>
        /// Set up a delegate with result that runs continuously after the task completes.
        /// </summary>
        /// <typeparam name="TResult">result type</typeparam>
        /// <param name="continuation">delegate</param>
        /// <param name="cancellationToken">cancel token</param>
        /// <returns>Task with result</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
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
                finally
                {
                    if (source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
            }

            return ContinuePromise<TResult>.Create(in this, continuation, in cancellationToken).Task;
        }

        /// <summary>
        /// Set up a delegate with state that runs continuously after the task completes.
        /// </summary>
        /// <param name="continuation">delegate</param>
        /// <param name="state">delegate parameter</param>
        /// <param name="cancellationToken">cancel token</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
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
                finally
                {
                    if (source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
            }

            return ContinueWithStatePromise.Create(in this, continuation, state, in cancellationToken).Task;
        }

        /// <summary>
        /// Set up a delegate with state with result that runs continuously after the task completes.
        /// </summary>
        /// <param name="continuation">delegate</param>
        /// <param name="state">delegate parameter</param>
        /// <param name="cancellationToken">cancel token</param>
        /// <returns>Task with result</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
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
                finally
                {
                    if (source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
            }

            return ContinueWithStatePromise<TResult>.Create(in this, continuation, state, in cancellationToken).Task;
        }

        internal sealed class ContinuePromise : ContinuePromiseBase<ContinuePromise>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ContinuePromise Create(in ETask prevTask, Action<ETask> continuation, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;

                return promise;
            }

            ETask prevTask;
            Action<ETask> continuation;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                try
                {
                    continuation.Invoke(prevTask);
                }
                finally
                {
                    if (prevTask.source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
                TrySetResult();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Reset()
            {
                base.Reset();
                prevTask = default;
                continuation = null;
            }
        }

        internal sealed class ContinueWithStatePromise : ContinuePromiseBase<ContinueWithStatePromise>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                try
                {
                    continuation.Invoke(prevTask, state);
                }
                finally
                {
                    if (prevTask.source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
                TrySetResult();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ContinuePromise<TResult> Create(in ETask prevTask, Func<ETask, TResult> continuation, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;

                return promise;
            }

            ETask prevTask;
            Func<ETask, TResult> continuation;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                TResult? result;
                try
                {
                    result = continuation.Invoke(prevTask);
                }
                finally
                {
                    if (prevTask.source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
                TrySetResult(result);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                TResult? result;
                try
                {
                    result = continuation.Invoke(prevTask, state);
                }
                finally
                {
                    if (prevTask.source is IPoolItem poolItem)
                        poolItem.Return(true);
                }
                TrySetResult(result);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
