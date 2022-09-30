using EasyTask.Pools;
using EasyTask.Sources;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EasyTask.Promises
{
    internal abstract class WhenPromise<TPromise> : ETaskCompletionSourceBase<TPromise>
        where TPromise : WhenPromise<TPromise>, new()
    {
        static readonly Action<object> InvokeOnTaskCompleted = OnTaskCompletedCallback;

        IReadOnlyList<ETask>? tasks;
        protected int TaskCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => tasks?.Count ?? 0;
        }
        protected int countCompleted;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TPromise Create(IReadOnlyList<ETask> tasks)
        {
            var promise = Rent();
            promise.Initialize(tasks);
            return promise;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Reset()
        {
            base.Reset();
            if (tasks is IPoolItem item)
                item.Return();
            tasks = null;
            countCompleted = 0;
        }
    }
}
