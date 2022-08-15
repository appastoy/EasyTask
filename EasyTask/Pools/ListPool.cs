using System.Buffers;
using System.Collections;
using System.Collections.Generic;

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
#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
        T[] array;
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
        
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
#pragma warning disable CS8625 // Null 리터럴을 null을 허용하지 않는 참조 형식으로 변환할 수 없습니다.
            array = null;
#pragma warning restore CS8625 // Null 리터럴을 null을 허용하지 않는 참조 형식으로 변환할 수 없습니다.
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
