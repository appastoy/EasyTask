using EasyTask.Sources;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EasyTask
{
    partial struct ETask<TResult>
    {
        /// <summary>
        /// Configure to capture current context.
        /// </summary>
        /// <param name="captureContext">Do you want to capture?</param>
        /// <returns>Awaitable</returns>
        public ConfigureAwaitable ConfigureAwait(bool captureContext)
        {
            if (!IsCompleted &&
                source is IConfigureAwaitable awaitable)
                awaitable.SetCaptureContext(captureContext);

            return new ConfigureAwaitable(in this);
        }

        public readonly struct ConfigureAwaitable
        {
            readonly ETask<TResult> task;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfigureAwaitable(in ETask<TResult> task) => this.task = task;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Awaiter GetAwaiter() => task.GetAwaiter();
        }
    }
}
