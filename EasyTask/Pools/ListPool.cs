using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace EasyTask.Pools
{
    internal static class ListPool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListItem<T> Rent<T>(int capacity) => ListItem<T>.Rent(capacity);
    }

    internal sealed class ListItem<T> : PoolItem<ListItem<T>>, IReadOnlyList<T>
    {
        T[] array;
        int size;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => array[index];
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListItem<T> Rent(int capacity)
        {
            var list = Rent();
            list.array = ArrayPool<T>.Shared.Rent(capacity);
            list.size = 0;
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ICollection<T> collection)
        {
            EnsureSize(collection.Count, true);
            collection.CopyTo(array, 0);
            size = collection.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            EnsureSize(size + 1);
            array[size++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Reset()
        {
            ArrayPool<T>.Shared.Return(array, true);
            array = null;
            size = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            return (array as IEnumerable<T>).GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
