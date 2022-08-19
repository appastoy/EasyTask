using EasyTask.Pools;
using System;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS8604

namespace EasyTask
{
    partial struct ETask<TResult>
    {
        static readonly Action<object> InvokeTaskSetResultDelegate = InvokeTaskSetResult;

        public ETask AsETask() => new ETask(source, token);

        public static implicit operator ETask(ETask<TResult> task) => task.AsETask();

        public ValueTask<TResult> AsValueTask()
        {
            if (source is null || IsCompletedSuccessfully)
                return default;

            return new ValueTask<TResult>(source, token);
        }

        public Task<TResult> AsTask()
        {
            if (source is null || IsCompletedSuccessfully)
                return Task.FromResult<TResult>(default);

            if (IsCanceled)
                return Task.FromCanceled<TResult>(Exception is OperationCanceledException oce ? 
                    oce.CancellationToken : new CancellationToken(true));

            if (IsFaulted)
                return Task.FromException<TResult>(Exception);

            var tcs = new TaskCompletionSource<TResult>();
            source.OnCompleted(InvokeTaskSetResultDelegate, TuplePool.Rent(tcs, this), token);
            
            return tcs.Task;
        }

        static void InvokeTaskSetResult(object obj)
        {
            using var tuple = (FieldTuple<TaskCompletionSource<TResult>, ETask<TResult>>)obj;
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
