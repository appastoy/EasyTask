using EasyTask.Pools;
using System;
using System.Collections.Generic;

namespace EasyTask.Promises
{
    internal abstract class WhenPromise<T> : Promise<T>
        where T : WhenPromise<T>, new()
    {
        static readonly Action<object> InvokeOnTaskCompleted = OnTaskCompletedCallback;

        IReadOnlyList<ETask>? tasks;
        protected int TaskCount => tasks?.Count ?? 0;

        public static T Create(IReadOnlyList<ETask> tasks)
        {
            var promise = Rent();
            promise.Initialize(tasks);
            return promise;
        }

        void Initialize(IReadOnlyList<ETask> tasks)
        {
            this.tasks = tasks;

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
                    awaiter.OnCompleted(InvokeOnTaskCompleted, TuplePool.Rent((T)this, awaiter));
                }
            }
        }

        static void OnTaskCompletedCallback(object obj)
        {
            using var tuple = (FieldTuple<T, ETask.Awaiter>)obj;
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


        protected override void BeforeReturn()
        {
            if (tasks is IListPoolItem item)
                item.Return();
            tasks = null;
        }
    }
}
