using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        static SynchronizationContext? mainThreadContext;
        public static SynchronizationContext? MainThreadContext => mainThreadContext;

        /// <summary>
        /// <para>Set main thread synchronization context.</para>
        /// <para>When it is assigned not null, You can use SwitchMainThread() method properly</para>
        /// <para>and unhandled exception is rethrown on main thread.</para>
        /// </summary>
        /// <param name="context">Main thread synchronization context. It can be null.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMainThreadContext(SynchronizationContext? context)
            => mainThreadContext = context;

        /// <summary>
        /// Switch to main thread.
        /// </summary>
        /// <returns>Awaitable</returns>
        /// <exception cref="InvalidOperationException">main thread context is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchSynchronizationContextAwaitable SwitchToMainThread()
        {
            if (mainThreadContext == null)
                throw new InvalidOperationException($"Main Thread SynchronizationContext is null. You should it to call ETask.{nameof(SetMainThreadContext)}().");

            return new SwitchSynchronizationContextAwaitable(mainThreadContext);
        }
    }
}
