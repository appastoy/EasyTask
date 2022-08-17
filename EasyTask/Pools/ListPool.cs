using System.Buffers;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace EasyTask.Pools
{
    internal static class ListPool
    {
        public static ListItem<T> Rent<T>(int capacity) => ListItem<T>.Rent(capacity);
    }

    internal interface IListPoolItem
    {
        void Return();
    }

    internal sealed class ListItem<T> : PoolItem<ListItem<T>>, IReadOnlyList<T>, IListPoolItem
    {
        T[] array;
        int size;

        public T this[int index] => array[index];

        public int Count => array.Length;

        public static ListItem<T> Rent(int capacity)
        {
            var list = Rent();
            list.array = ArrayPool<T>.Shared.Rent(capacity);
            list.size = 0;
            return list;
        }

        public void CopyFrom(ICollection<T> collection)
        {
            EnsureSize(collection.Count, true);
            collection.CopyTo(array, 0);
            size = collection.Count;
        }

        public void Add(in T item)
        {
            EnsureSize(size + 1);
            array[size++] = item;
        }

        void EnsureSize(int needSize, bool skipCopy = false)
        {
            if (needSize > array.Length)
            {
                var newSize = array.Length * 2;
                if (newSize < needSize)
                    newSize = needSize * 2;
                var newArray = ArrayPool<T>.Shared.Rent(newSize);
                if (!skipCopy)
                    array.CopyTo(newArray, 0);
                ArrayPool<T>.Shared.Return(array, true);
                array = newArray;
                size = newSize;
            }
        }

        protected override void BeforeReturn()
        {
            ArrayPool<T>.Shared.Return(array, true);
            array = null;
            size = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (array as IEnumerable<T>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
