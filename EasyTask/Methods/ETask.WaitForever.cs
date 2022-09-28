using System;
using System.Runtime.CompilerServices;

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Wait forever.
        /// </summary>
        /// <returns>Awaitable</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForeverAwaitable WaitForever() => default;

        public readonly struct WaitForeverAwaitable
        {
            public Awaiter GetAwaiter() => default;

            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                public bool IsCompleted => false;

                public void GetResult() { }

                public void OnCompleted(Action continuation)
                {
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                }
            }
        }
    }
}
