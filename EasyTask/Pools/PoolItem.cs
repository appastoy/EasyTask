using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask.Pools
{
    public class PoolItem<TItem> : IDisposable
        where TItem : PoolItem<TItem>, new()
    {
        static TItem? poolRoot;
        static int @lock;

        public static int RentCount { get; private set; }
        public static int FreeCount { get; private set; }
        public static int TotalCount => RentCount + FreeCount;

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
                FreeCount--;
            }
            rentItem.BeforeRent();
            rentItem.isRented = true;
            RentCount++;
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
            FreeCount++;
            RentCount--;
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
