using System.Runtime.CompilerServices;

namespace EasyTask
{
    partial struct ETask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask<T> FromResult<T>(T result) => new (result);
    }
}
