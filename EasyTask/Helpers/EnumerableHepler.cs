using EasyTask.Pools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTask.Helpers
{
    internal static class EnumerableHepler
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> @this)
        {
            if (@this is IReadOnlyList<T> list)
                return list;

            if (@this is ICollection<T> collection)
            {
                if (collection.Count == 0)
                    return Array.Empty<T>();

                var tempCollection = ListPool.Rent<T>(collection.Count);
                tempCollection.CopyFrom(collection);
                return tempCollection;
            }

            if (!@this.Any())
                return Array.Empty<T>();

            var tempList = ListPool.Rent<T>(32);
            foreach (var item in @this)
                tempList.Add(item);

            return tempList;
        }
    }
}
