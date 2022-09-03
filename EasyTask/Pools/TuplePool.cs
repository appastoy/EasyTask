using System;

#pragma warning disable CS8601
#pragma warning disable CS8618

namespace EasyTask.Pools
{
    internal static class TuplePool
    {
        public static FieldTuple<T1> Rent<T1>(T1 _1)
        {
            var tuple = FieldTuple<T1>.Rent();
            tuple._1 = _1;
            return tuple;
        }

        public static FieldTuple<T1, T2> Rent<T1, T2>(T1 _1, T2 _2)
        {
            var tuple = FieldTuple<T1, T2>.Rent();
            tuple._1 = _1;
            tuple._2 = _2;
            return tuple;
        }

        public static FieldTuple<T1, T2, T3> Rent<T1, T2, T3>(T1 _1, T2 _2, T3 _3)
        {
            var tuple = FieldTuple<T1, T2, T3>.Rent();
            tuple._1 = _1;
            tuple._2 = _2;
            tuple._3 = _3;
            return tuple;
        }

        public static FieldTuple<T1, T2, T3, T4> Rent<T1, T2, T3, T4>(T1 _1, T2 _2, T3 _3, T4 _4)
        {
            var tuple = FieldTuple<T1, T2, T3, T4>.Rent();
            tuple._1 = _1;
            tuple._2 = _2;
            tuple._3 = _3;
            tuple._4 = _4;
            return tuple;
        }
    }

    internal sealed class FieldTuple<T1>
        : PoolItem<FieldTuple<T1>>, IDisposable
    {
        public T1 _1;

        protected override void Reset()
        {
            _1 = default;
        }
    }

    internal sealed class FieldTuple<T1, T2>
        : PoolItem<FieldTuple<T1, T2>>, IDisposable
    {
        public T1 _1;
        public T2 _2;

        protected override void Reset()
        {
            _1 = default;
            _2 = default;
        }
    }

    internal sealed class FieldTuple<T1, T2, T3> 
        : PoolItem<FieldTuple<T1, T2, T3>>
    {
        public T1 _1;
        public T2 _2;
        public T3 _3;

        protected override void Reset()
        {
            _1 = default;
            _2 = default;
            _3 = default;
        }
    }

    internal sealed class FieldTuple<T1, T2, T3, T4> 
        : PoolItem<FieldTuple<T1, T2, T3, T4>>
    {
        public T1 _1;
        public T2 _2;
        public T3 _3;
        public T4 _4;

        protected override void Reset()
        {
            _1 = default;
            _2 = default;
            _3 = default;
            _4 = default;
        }
    }
}