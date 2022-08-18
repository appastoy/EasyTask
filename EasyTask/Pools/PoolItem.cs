using System;
using System.Threading;

namespace EasyTask.Pools
{

    public class PoolItem<TItem> : IDisposable
        where TItem : PoolItem<TItem>, new()
    {
        static TItem? poolRoot;
        static int poolSize;
        static int @lock;

        public static TItem Rent()
        {
            using var _ = ScopeLocker.Lock();

            TItem rentItem;
            if (poolRoot is null)
            {
                rentItem = new TItem();
            }
            else
            {
                rentItem = poolRoot;
                poolRoot = poolRoot.next;
                rentItem.next = null;
                poolSize--;
            }
            rentItem.BeforeRent();
            rentItem.isRented = true;
            return rentItem;
        }

        bool isRented;
        TItem? next;

        public void Return()
        {
            if (!isRented)
                return;

            using var _ = ScopeLocker.Lock();
         
            Reset();
            isRented = false;
            next = poolRoot;
            poolRoot = (TItem)this;
            poolSize++;
        }

        protected virtual void BeforeRent() { }
        protected virtual void Reset() { }

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
