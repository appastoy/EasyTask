using EasyTask.Pools;
using EasyTask.Sources;
using System;
using System.Collections.Generic;

namespace EasyTask.Promises
{
    internal abstract class WhenPromise<TPromise, TResult, TWhenResult> : ETaskCompletionSourceBase<TPromise, TWhenResult>
        where TPromise : WhenPromise<TPromise, TResult, TWhenResult>, new()
    {
        static readonly Action<object> InvokeOnTaskCompleted = OnTaskCompletedCallback;

        IReadOnlyList<ETask<TResult>>? tasks;
        protected int taskCount => tasks?.Count ?? 0;
        protected int countCompleted;

        public static TPromise Create(IReadOnlyList<ETask<TResult>> tasks)
        {
            var promise = Rent();
            promise.Initialize(tasks);
            return promise;
        }

        void Initialize(IReadOnlyList<ETask<TResult>> tasks)
        {
            this.tasks = tasks;
            countCompleted = 0;

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                ETask<TResult>.Awaiter awaiter;
                try
                {
                    awaiter = task.GetAwaiter();
                }
                catch (Exception exception)
                {
                    TrySetException(exception);
                    return;
                }

                if (awaiter.IsCompleted)
                {
                    OnTaskCompleted(in awaiter, i);
                }
                else
                {
                    awaiter.OnCompleted(InvokeOnTaskCompleted, TuplePool.Rent((TPromise)this, awaiter, i));
                }
            }
        }

        static void OnTaskCompletedCallback(object obj)
        {
            using var tuple = (FieldTuple<TPromise, ETask<TResult>.Awaiter, int>)obj;
            tuple._1.OnTaskCompleted(in tuple._2, tuple._3);
        }

        protected virtual void OnTaskCompleted(in ETask<TResult>.Awaiter awaiter, int index) { }

        protected override void Reset()
        {
            base.Reset();
            if (tasks is IListPoolItem item)
                item.Return();
            tasks = null;
            countCompleted = 0;
        }
    }
}
