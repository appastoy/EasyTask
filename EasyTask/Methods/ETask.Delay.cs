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

        internal sealed class DelayPromise : ETaskCompletionSourceBase<DelayPromise>
        {
            static readonly Action<DelayPromise> InvokeDelayCheck = DelayCheck;
            static readonly Action<DelayPromise> InvokeDelayCheckWithCancel = DelayCheckWithCancel;

            public static DelayPromise Create(TimeSpan duration, CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.cancellationToken = cancellationToken;
                promise.endTime = DateTime.UtcNow.Add(duration);
                if (cancellationToken == CancellationToken.None)
                    ThreadPool.QueueUserWorkItem(InvokeDelayCheck, promise, false);
                else
                    ThreadPool.QueueUserWorkItem(InvokeDelayCheckWithCancel, promise, false);
                return promise;
            }

            DateTime endTime;
            CancellationToken cancellationToken;

            protected override void Reset()
            {
                base.Reset();
                cancellationToken = default;
            }

            static void DelayCheck(DelayPromise promise)
            {
                var duration = promise.endTime - DateTime.UtcNow;
                if (duration > TimeSpan.Zero)
                    Thread.Sleep(duration);
                promise.TrySetResult();
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
                    promise.TrySetResult();
                }
                catch (OperationCanceledException cancelException)
                {
                    promise.TrySetCanceled(cancelException);
                }
            }
        }
    }
}
