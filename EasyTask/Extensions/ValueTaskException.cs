using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EasyTask
{
    internal static class ValueTaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ETask AsETask(this ValueTask task) => await task;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ETask<T> AsETask<T>(this ValueTask<T> task) => await task;
    }
}
