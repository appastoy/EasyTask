using System;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS8600
#pragma warning disable CS8603

namespace EasyTask
{
    internal static class TaskExtensions
    {
        public static ETask AsETask(this Task task, bool captureContext = true)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task.IsCompletedSuccessfully)
                return ETask.CompletedTask;

            if (task.IsFaulted)
                return ETask.FromException(task.Exception);

            if (task.IsCanceled)
                return ETask.FromCanceled(GetOperationCanceledException(task.Exception));

            var ecs = ETaskCompletionSource.Rent();
            task.ContinueWith(
                InvokeOnContinueWith
                , ecs
                , captureContext ? 
                    TaskScheduler.FromCurrentSynchronizationContext() : 
                    TaskScheduler.Current);
            
            return ecs.Task;
        }

        public static ETask<T> AsETask<T>(this Task<T> task, bool captureContext = true)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task.IsCompletedSuccessfully)
                return ETask.FromResult(task.Result);

            if (task.IsFaulted)
                return ETask.FromException<T>(task.Exception);

            if (task.IsCanceled)
                return ETask.FromCanceled<T>(GetOperationCanceledException(task.Exception));

            var ecs = ETaskCompletionSource<T>.Rent();
            task.ContinueWith(
                TaskDelegateCache<T>.InvokeOnContinueWith
                , ecs
                , captureContext ?
                    TaskScheduler.FromCurrentSynchronizationContext() :
                    TaskScheduler.Current);

            return ecs.Task;
        }

        static OperationCanceledException GetOperationCanceledException(AggregateException exception)
            => exception.InnerException is OperationCanceledException oce ? oce :
               exception.InnerExceptions
                    .Select(e => e as OperationCanceledException)
                    .FirstOrDefault(e => e != null) ??
                    (OperationCanceledException)ETask.CanceledTask.Exception;

        static readonly Action<Task, object> InvokeOnContinueWith = OnContinueWith;

        static void OnContinueWith(Task task, object obj)
        {
            var promise = (ETaskCompletionSource)obj;

            try
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        promise.TrySetCanceled(GetOperationCanceledException(task.Exception));
                        break;

                    case TaskStatus.Faulted:
                        promise.TrySetException(task.Exception);
                        break;

                    case TaskStatus.RanToCompletion:
                        promise.TrySetResult();
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            finally
            {
                promise.Return();
            }
        }

        internal static class TaskDelegateCache<T>
        {
            public static readonly Action<Task<T>, object> InvokeOnContinueWith = OnContinueWith;

            static void OnContinueWith(Task<T> task, object obj)
            {
                var promise = (ETaskCompletionSource<T>)obj;

                try
                {
                    switch (task.Status)
                    {
                        case TaskStatus.Canceled:
                            promise.TrySetCanceled(GetOperationCanceledException(task.Exception));
                            break;

                        case TaskStatus.Faulted:
                            promise.TrySetException(task.Exception);
                            break;

                        case TaskStatus.RanToCompletion:
                            promise.TrySetResult(task.Result);
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }
                finally
                {
                    promise.Return();
                }
            }
        }
    }
}