﻿using EasyTask.Pools;
using System;

namespace EasyTask
{
    partial struct ETask
    {
        static readonly Action<object> InvokeProcessOnCompleted = OnProcessOnCompleted;

        public void Forget()
        {
            var awaiter = GetAwaiter();
            if (awaiter.IsCompleted)
            {
                ProcessOnCompleted(awaiter);
            }
            else
            {
                awaiter.OnCompleted(InvokeProcessOnCompleted, TuplePool.Rent(awaiter));
            }
        }

        static void OnProcessOnCompleted(object obj)
        {
            using var tuple = (FieldTuple<Awaiter>)obj;
            ProcessOnCompleted(in tuple._1);
        }

        static void ProcessOnCompleted(in Awaiter awaiter)
        {
            try
            {
                awaiter.GetResult();
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception exception)
            {
                PublishUnhandledException(exception);
            }
        }
    }
}
