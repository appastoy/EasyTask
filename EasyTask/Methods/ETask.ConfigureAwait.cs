using EasyTask.Sources;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// <para>Configure to capture context. To capture context is true by default.</para>
        /// <para>If not capture, Task can't guarantee that post action runs in the same thread as current.</para>
        /// <para>Mainly it is used for performance purpose.</para>
        /// </summary>
        /// <param name="captureContext">Capture SynchronizationContext.Current</param>
        /// <returns>ConfigureAwaitable</returns>
        public ConfigureAwaitable ConfigureAwait(bool captureContext)
        {
            if (!IsCompleted &&
                source is IConfigureAwaitable awaitable)
                awaitable.SetCaptureContext(captureContext);

            return new ConfigureAwaitable(in this);
        }

        public readonly struct ConfigureAwaitable
        {
            readonly ETask task;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfigureAwaitable(in ETask task) => this.task = task;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Awaiter GetAwaiter() => task.GetAwaiter();
        }
    }
}
