using System;
using System.Threading.Tasks;

namespace EasyTask
{
    partial struct ETask
    {
        static readonly Action<object?> InvokeTaskSetResultDelegate = InvokeTaskSetResult;

        public ValueTask ToValueTask()
            => source is null ? default : new ValueTask(source, token);

        public Task ToTask()
        {
            if (source is null)
                return Task.CompletedTask;

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
