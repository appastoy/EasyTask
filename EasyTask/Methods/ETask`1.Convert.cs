using EasyTask.Pools;
using System;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS8604

namespace EasyTask
{
    partial struct ETask<T>
    {
        static readonly Action<object> InvokeTaskSetResultDelegate = InvokeTaskSetResult;

        public ETask AsETask() => new ETask(source, token);

        public static implicit operator ETask(ETask<T> task) => task.AsETask();

        public ValueTask<T> AsValueTask()
        {
            if (source is null || IsCompletedSuccessfully)
                return default;

            return new ValueTask<T>(source, token);
        }

        public Task<T> AsTask()
        {
            if (source is null || IsCompletedSuccessfully)
                return Task.FromResult<T>(default);

            if (IsCanceled)
                return Task.FromCanceled<T>(Exception is OperationCanceledException oce ? 
                    oce.CancellationToken : new CancellationToken(true));

            if (IsFaulted)
                return Task.FromException<T>(Exception);

            var tcs = new TaskCompletionSource<T>();
            source.OnCompleted(InvokeTaskSetResultDelegate, TuplePool.Rent(tcs, this), token);
            
            return tcs.Task;
        }

        static void InvokeTaskSetResult(object obj)
        {
            using var tuple = (FieldTuple<TaskCompletionSource<T>, ETask<T>>)obj;
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
