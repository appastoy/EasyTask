using EasyTask.Sources;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask.Promises;

internal abstract class WaitPromise<TWaitPromise> : ETaskCompletionSourceBase<TWaitPromise>
    where TWaitPromise : WaitPromise<TWaitPromise>, new()
{
    static readonly Action<TWaitPromise> InvokeWaitUntilCheck = WaitCheck;
    static readonly Action<TWaitPromise> InvokeWaitUntilCheckWithCancel = WaitCheckWithCancel;

    DateTime endTime;
    CancellationToken cancellationToken;

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnInitialize(TimeSpan timeout, CancellationToken cancellationToken)
    {
        this.cancellationToken = cancellationToken;
        endTime = timeout.Ticks > 0 ? DateTime.UtcNow.Add(timeout) : DateTime.MaxValue;
        if (cancellationToken == CancellationToken.None)
            ThreadPool.QueueUserWorkItem(InvokeWaitUntilCheck, (TWaitPromise)this, false);
        else
            ThreadPool.QueueUserWorkItem(InvokeWaitUntilCheckWithCancel, (TWaitPromise)this, false);
    }

    protected abstract bool CheckWaitingEnd();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Reset()
    {
        base.Reset();
        endTime = default;
        cancellationToken = default;
    }

    static void WaitCheck(TWaitPromise promise)
    {
        try
        {
            while (!promise.CheckWaitingEnd() &&
                   DateTime.UtcNow < promise.endTime)
                Thread.Yield();
            promise.TrySetResult();
        }
        catch (Exception ex)
        {
            promise.TrySetException(ex);
        }
    }

    static void WaitCheckWithCancel(TWaitPromise promise)
    {
        try
        {
            promise.cancellationToken.ThrowIfCancellationRequested();

            while (!promise.CheckWaitingEnd() &&
               DateTime.UtcNow < promise.endTime)
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
        catch (Exception ex)
        {
            promise.TrySetException(ex);
        }
    }
}