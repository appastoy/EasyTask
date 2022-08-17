using EasyTask.Helpers;
using EasyTask.Pools;
using EasyTask.Sources;
using System;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static ETask Delay(TimeSpan duration, CancellationToken cancellationToken = default)
        {
            return DelayPromise.Create(duration, cancellationToken).Task;
        }

        public static ETask Delay(int milliseconds, CancellationToken cancellationToken = default)
        {
            return Delay(new TimeSpan(milliseconds * TimeSpan.TicksPerMillisecond), cancellationToken);
        }

        internal sealed class DelayPromise : ETaskCompletionSourceGeneric<DelayPromise>
        {
            static readonly Action<DelayPromise> InvokeDelayCheck = DelayCheck;
            static readonly Action<DelayPromise> InvokeDelayCheckWithCancel = DelayCheckWithCancel;
            static readonly SendOrPostCallback InvokeOnTrySetCanceled = OnTrySetCanceled;

            public static DelayPromise Create(TimeSpan duration, CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.cancellationToken = cancellationToken;
                promise.endTime = DateTime.UtcNow.Add(duration);
                promise.sync = SynchronizationContext.Current;
                if (cancellationToken == CancellationToken.None)
                    ThreadPool.QueueUserWorkItem(InvokeDelayCheck, promise, false);
                else
                    ThreadPool.QueueUserWorkItem(InvokeDelayCheckWithCancel, promise, false);
                return promise;
            }

            readonly Action invokeTrySetResult;

            SynchronizationContext? sync;
            DateTime endTime;
            CancellationToken cancellationToken;

            public DelayPromise() => invokeTrySetResult = TrySetResult;

            protected override void BeforeReturn()
            {
                sync = null;
                cancellationToken = default;
            }

            void TrySetResultWithSync()
            {
                if (sync != null)
                    sync.Post(DelegateCache.InvokeAsSendOrPostCallback, invokeTrySetResult);
                else
                    TrySetResult();
            }

            void TrySetCanceledWithSync(OperationCanceledException canceledException)
            {
                if (sync != null)
                    sync.Post(InvokeOnTrySetCanceled, TuplePool.Rent(this, canceledException));
                else
                    TrySetCanceled(canceledException);
            }

            static void DelayCheck(DelayPromise promise)
            {
                var duration = promise.endTime - DateTime.UtcNow;
                if (duration > TimeSpan.Zero)
                    Thread.Sleep(duration);
                promise.TrySetResultWithSync();
            }

            static void DelayCheckWithCancel(DelayPromise promise)
            {
                try
                {
                    promise.cancellationToken.ThrowIfCancellationRequested();

                    while (DateTime.UtcNow < promise.endTime)
                    {
                        Thread.Yield();
                        promise.cancellationToken.ThrowIfCancellationRequested();
                    }
                    promise.TrySetResultWithSync();
                }
                catch (OperationCanceledException cancelException)
                {
                    promise.TrySetCanceledWithSync(cancelException);
                }
            }

            static void OnTrySetCanceled(object obj)
            {
                using var tuple = (FieldTuple<DelayPromise, OperationCanceledException>)obj;
                tuple._1.TrySetCanceled(tuple._2);
            }
        }
    }
}
