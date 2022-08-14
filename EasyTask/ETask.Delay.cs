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

        internal sealed class DelayPromise : Promise<DelayPromise>
        {
            static readonly Action<DelayPromise> InvokeDelayCheck = DelayCheck;
            static readonly Action<DelayPromise> InvokeDelayCheckWithCancel = DelayCheckWithCancel;

            public static DelayPromise Create(TimeSpan duration, CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.Reset();
                promise.sync = SynchronizationContext.Current;
                promise.endTime = DateTime.UtcNow.Add(duration);
                promise.cancellationToken = cancellationToken;
                if (cancellationToken == CancellationToken.None)
                    ThreadPool.QueueUserWorkItem(InvokeDelayCheck, promise, false);
                else
                    ThreadPool.QueueUserWorkItem(InvokeDelayCheckWithCancel, promise, false);

                return promise;
            }

            SynchronizationContext? sync;
            DateTime endTime;
            CancellationToken cancellationToken;
            Action trySetResultAction;

            public ETask Task => new ETask(this, Token);

            public DelayPromise() => trySetResultAction = TrySetResult;

            protected override void BeforeReturn()
            {
                sync = null;
                cancellationToken = CancellationToken.None;
            }

            void TrySetResultWithSync()
            {
                if (sync != null)
                    sync.Post(DelegateCache.InvokeAsSendOrPostCallback, trySetResultAction);
                else
                    TrySetResult();
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
                promise.cancellationToken.ThrowIfCancellationRequested();

                while (DateTime.UtcNow < promise.endTime)
                {
                    Thread.Yield();
                    promise.cancellationToken.ThrowIfCancellationRequested();
                }
                promise.TrySetResultWithSync();
            }
        }
    }
}
