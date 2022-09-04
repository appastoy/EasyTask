using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTask
{
    partial struct ETask
    {
        static readonly Action<object?> InvokeTaskSetResultDelegate = InvokeTaskSetResult;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask AsValueTask()
        {
            if (source == null || IsCompletedSuccessfully)
                return default;

            return new ValueTask(source, token);
        }

        public Task AsTask()
        {
            if (source is null || IsCompletedSuccessfully)
                return Task.CompletedTask;

            if (IsCanceled)
                return Task.FromCanceled(Exception is OperationCanceledException oce ? 
                    oce.CancellationToken : new CancellationToken(true));

            if (IsFaulted)
                return Task.FromException(Exception);

            var taskCompletionSource = new TaskCompletionSource<object?>();
            source.OnCompleted(InvokeTaskSetResultDelegate, taskCompletionSource, token);
            return taskCompletionSource.Task;
        }

        static void InvokeTaskSetResult(object? obj)
        {
            if (obj is TaskCompletionSource<object?> taskCompletionSource)
                taskCompletionSource.SetResult(null);
        }
    }
}
