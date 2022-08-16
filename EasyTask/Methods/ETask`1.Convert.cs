using EasyTask.Pools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTask
{
    partial struct ETask<T>
    {
        static readonly Action<object?> InvokeTaskSetResultDelegate = InvokeTaskSetResult;

        public ETask AsETask() => new ETask(source, token);

        public ValueTask<T> AsValueTask()
        {
            if (source is null || IsCompletedSuccessfully)
                return default;

            return new ValueTask<T>(source, token);
        }

        public Task<T> AsTask()
        {
            if (source is null || IsCompletedSuccessfully)
#pragma warning disable CS8604 // 가능한 null 참조 인수입니다.
                return Task.FromResult<T>(default);
#pragma warning restore CS8604 // 가능한 null 참조 인수입니다.

            if (IsCanceled)
                return Task.FromCanceled<T>(Exception is OperationCanceledException oce ? 
                    oce.CancellationToken : new CancellationToken(true));

            if (IsFaulted)
                return Task.FromException<T>(Exception);

            var taskCompletionSource = new TaskCompletionSource<T>();
            source.OnCompleted(InvokeTaskSetResultDelegate, TuplePool.Rent(taskCompletionSource, this), token);
            
            return taskCompletionSource.Task;
        }

        static void InvokeTaskSetResult(object? obj)
        {
            if (obj is FieldTuple<TaskCompletionSource<T>, ETask<T>> tuple)
            {
                using var _ = tuple;
                try
                {
                    tuple._1.TrySetResult(tuple._2.Result);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException oce)
                        tuple._1.TrySetCanceled(oce.CancellationToken);
                    else
                        tuple._1.TrySetException(ex);
                }
            }
        }
    }
}
