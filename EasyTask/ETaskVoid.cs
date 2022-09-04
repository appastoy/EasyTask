using EasyTask.CompilerServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EasyTask
{
    /// <summary>
    /// <para>Lightweight task object for zero allocation. It is useful than ETask when it doesn't need to await.</para>
    /// <para>If an exception is thrown when task is invoking, It is [passed via ETask.UnhandledExceptionHandler] or [rethrown (If ETask.UnhandledExceptionHandler is null)] on the main thread.</para>
    /// <para>The main thread context is null by default. You can set the main thread via ETask.SetMainThreadContext().</para>
    /// </summary>
    [StructLayout(LayoutKind.Auto, Pack = 1)]
    [AsyncMethodBuilder(typeof(ETaskVoidMethodBuilder))]
    public readonly struct ETaskVoid
    {
        /// <summary>
        /// Actually, this method is nothing to do. It used for avoid compile warning.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Forget() { }
    }
}
