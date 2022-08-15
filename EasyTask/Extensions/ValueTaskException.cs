using System.Threading.Tasks;

namespace EasyTask
{
    internal static class ValueTaskExtensions
    {
        public static async ETask AsETask(this ValueTask task) => await task;
        public static async ETask<T> AsETask<T>(this ValueTask<T> task) => await task;
    }
}
