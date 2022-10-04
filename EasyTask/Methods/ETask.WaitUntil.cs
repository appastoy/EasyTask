using EasyTask.Promises;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Wait until func returns true. (not thread safe)
        /// </summary>
        /// <param name="func">Check waiting end func</param>
        /// <param name="timeout">timeout (timeout <= 0 : infinite)</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask WaitUntil(Func<bool> func, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return WaitUntilPromise.Create(func, timeout, cancellationToken).Task;
        }

        internal sealed class WaitUntilPromise : WaitPromise<WaitUntilPromise>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static WaitUntilPromise Create(Func<bool> func, TimeSpan timeout, CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.func = func;
                promise.OnInitialize(timeout, cancellationToken);
                return promise;
            }

            Func<bool>? func;

            protected override bool CheckWaitingEnd() => func!.Invoke();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Reset()
            {
                base.Reset();
                func = default;
            }
        }
    }
}
