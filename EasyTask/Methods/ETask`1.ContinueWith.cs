using EasyTask.Promises;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace EasyTask
{
    partial struct ETask<TResult>
    {
        /// <summary>
        /// Set continuation task with result.
        /// </summary>
        /// <param name="continuation">Continuation action</param>
        /// <param name="cancellationToken">Cancellation token before execute</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
        public ETask ContinueWith(Action<ETask<TResult>> continuation, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    continuation.Invoke(this);
                    return ETask.CompletedTask;
                }
                catch (Exception exception)
                {
                    return ETask.FromException(exception);
                }
            }

            return ContinuePromise.Create(in this, continuation, in cancellationToken).Task;
        }

        /// <summary>
        /// Set continuation with new result.
        /// </summary>
        /// <typeparam name="TNewResult">New result type.</typeparam>
        /// <param name="continuation">Continuation func</param>
        /// <param name="cancellationToken">Cancellation token before execute</param>
        /// <returns>Task with new result</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
        public ETask<TNewResult> ContinueWith<TNewResult>(Func<ETask<TResult>, TNewResult> continuation, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    return ETask.FromResult(continuation.Invoke(this));
                }
                catch (Exception exception)
                {
                    return ETask.FromException<TNewResult>(exception);
                }
            }

            return ContinuePromise<TNewResult>.Create(in this, continuation, in cancellationToken).Task;
        }

        /// <summary>
        /// Set continuation with state.
        /// </summary>
        /// <param name="continuation">Continuation action</param>
        /// <param name="state">Continuation state</param>
        /// <param name="cancellationToken">Cancellation token before execute</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
        public ETask ContinueWith(Action<ETask<TResult>, object> continuation, object state, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    continuation.Invoke(this, state);
                    return ETask.CompletedTask;
                }
                catch (Exception exception)
                {
                    return ETask.FromException(exception);
                }
            }

            return ContinueWithStatePromise.Create(in this, continuation, state, in cancellationToken).Task;
        }

        /// <summary>
        /// Set continuation with new result with state.
        /// </summary>
        /// <typeparam name="TNewResult">New result type</typeparam>
        /// <param name="continuation">Continuation func</param>
        /// <param name="state">Continuation state</param>
        /// <param name="cancellationToken">Cancellation token before execute.</param>
        /// <returns>Task with new result</returns>
        /// <exception cref="ArgumentNullException">continuation is null</exception>
        public ETask<TNewResult> ContinueWith<TNewResult>(Func<ETask<TResult>, object, TNewResult> continuation, object state, CancellationToken cancellationToken = default)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            if (IsCompleted)
            {
                try
                {
                    return ETask.FromResult(continuation.Invoke(this, state));
                }
                catch (Exception exception)
                {
                    return ETask.FromException<TNewResult>(exception);
                }
            }

            return ContinueWithStatePromise<TNewResult>.Create(in this, continuation, state, in cancellationToken).Task;
        }

        internal sealed class ContinuePromise : ContinuePromiseBase<ContinuePromise>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ContinuePromise Create(in ETask<TResult> prevTask, Action<ETask<TResult>> continuation, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;

                return promise;
            }

            ETask<TResult> prevTask;
            Action<ETask<TResult>> continuation;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                continuation.Invoke(prevTask);
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
            public static ContinueWithStatePromise Create(in ETask<TResult> prevTask, Action<ETask<TResult>, object> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.state = state;

                return promise;
            }

            ETask<TResult> prevTask;
            Action<ETask<TResult>, object> continuation;
            object state;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                continuation.Invoke(prevTask, state);
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

        internal sealed class ContinuePromise<TNewResult> 
            : ContinuePromiseBase<ContinuePromise<TNewResult>, TNewResult>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ContinuePromise<TNewResult> Create(in ETask<TResult> prevTask, Func<ETask<TResult>, TNewResult> continuation, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;

                return promise;
            }

            ETask<TResult> prevTask;
            Func<ETask<TResult>, TNewResult> continuation;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                var result = continuation.Invoke(prevTask);
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

        

        internal sealed class ContinueWithStatePromise<TNewResult> 
            : ContinuePromiseBase<ContinueWithStatePromise<TNewResult>, TNewResult>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ContinueWithStatePromise<TNewResult> Create(in ETask<TResult> prevTask, Func<ETask<TResult>, object, TNewResult> continuation, object state, in CancellationToken cancellationToken)
            {
                var promise = Create(prevTask.source, prevTask.token, in cancellationToken);
                promise.prevTask = prevTask;
                promise.continuation = continuation;
                promise.state = state;
                
                return promise;
            }

            ETask<TResult> prevTask;
            Func<ETask<TResult>, object, TNewResult> continuation;
            object state;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void OnTrySetResult()
            {
                var result = continuation.Invoke(prevTask, state);
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
