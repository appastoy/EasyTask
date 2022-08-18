using EasyTask.Pools;
using EasyTask.Sources;
using System;
using System.Collections.Generic;

namespace EasyTask.Promises
{
    internal abstract class WhenPromise<TPromise> : ETaskCompletionSourceGeneric<TPromise>
        where TPromise : WhenPromise<TPromise>, new()
    {
        static readonly Action<object> InvokeOnTaskCompleted = OnTaskCompletedCallback;

        IReadOnlyList<ETask>? tasks;
        protected int taskCount => tasks?.Count ?? 0;
        protected int countCompleted;

        public static TPromise Create(IReadOnlyList<ETask> tasks)
        {
            var promise = Rent();
            promise.Initialize(tasks);
            return promise;
        }

        void Initialize(IReadOnlyList<ETask> tasks)
        {
            this.tasks = tasks;
            countCompleted = 0;

            foreach (var task in tasks)
            {
                ETask.Awaiter awaiter;
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
                    OnTaskCompleted(in awaiter);
                }
                else
                {
                    awaiter.OnCompleted(InvokeOnTaskCompleted, TuplePool.Rent((TPromise)this, awaiter));
                }
            }
        }

        static void OnTaskCompletedCallback(object obj)
        {
            using var tuple = (FieldTuple<TPromise, ETask.Awaiter>)obj;
            tuple._1.OnTaskCompleted(tuple._2);
        }

        void OnTaskCompleted(in ETask.Awaiter awaiter)
        {
            try
            {
                awaiter.GetResult();
            }
            catch (Exception exception)
            {
                TrySetException(exception);
                return;
            }

            if (CheckCompleted())
                TrySetResult();
        }

        protected abstract bool CheckCompleted();

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
