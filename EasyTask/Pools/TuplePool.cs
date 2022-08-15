using System;

namespace EasyTask.Pools
{
    internal static class TuplePool
    {
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

#pragma warning disable CS8601 // 가능한 null 참조 할당입니다.
#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.

    internal sealed class FieldTuple<T1, T2>
        : PoolItem<FieldTuple<T1, T2>>, IDisposable
    {
        public T1 _1;
        public T2 _2;

        protected override void BeforeReturn()
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

        protected override void BeforeReturn()
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

        protected override void BeforeReturn()
        {
            _1 = default;
            _2 = default;
            _3 = default;
            _4 = default;
        }
    }

#pragma warning restore CS8601 // 가능한 null 참조 할당입니다.
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
}
