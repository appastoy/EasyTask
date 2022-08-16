using System;
using System.Threading;

namespace EasyTask.Pools
{

    internal class PoolItem<TItem> : IDisposable
        where TItem : PoolItem<TItem>, new()
    {
        static TItem? poolRoot;
        static int poolSize;
        static int @lock;

        public static TItem Rent()
        {
            using var _ = ScopeLocker.Lock();

            if (poolRoot is null)
                return new TItem();

            var rentItem = poolRoot;
            poolRoot = poolRoot.next;
            rentItem.next = null;
            poolSize--;
            rentItem.BeforeRent();
            return rentItem;
        }

        TItem? next;

        protected virtual void BeforeRent() { }

        public void Return()
        {
            using var _ = ScopeLocker.Lock();
         
            BeforeReturn();
            next = poolRoot;
            poolRoot = (TItem)this;
            poolSize++;
        }

        protected virtual void BeforeReturn() { }

        public void Dispose() => Return();

        readonly struct ScopeLocker : IDisposable
        {
            public static ScopeLocker Lock()
            {
                while (Interlocked.CompareExchange(ref @lock, 1, 0) != 0)
                    Thread.Yield();
                
                return default;
            }

            public void Dispose() => Volatile.Write(ref @lock, 0);
        }
    }
}
