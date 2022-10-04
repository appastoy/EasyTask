using EasyTask.Promises;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask
{
    partial struct ETask
    {
        /// <summary>
        /// Wait until func value has changed. (not thread safe)
        /// </summary>
        /// <param name="valueGetter">Get value func</param>
        /// <param name="initialValue">Initial value</param>
        /// <param name="equalityComparer">Equality comparer. If null, use default.</param>
        /// <param name="timeout">timeout (timeout <= 0 : infinite)</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask WaitUntilValueChanged<T>(Func<T> valueGetter, T initialValue, IEqualityComparer<T>? equalityComparer = default, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return WaitUntilValueChangedPromise<T>.Create(valueGetter, initialValue, equalityComparer, timeout, cancellationToken).Task;
        }

        /// <summary>
        /// Wait until func value has changed. The initial value is provided by valueGetter func. (not thread safe)
        /// </summary>
        /// <param name="valueGetter">Get value func</param>
        /// <param name="equalityComparer">Equality comparer. If null, use default.</param>
        /// <param name="timeout">timeout (timeout <= 0 : infinite)</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Task</returns>
        public static ETask WaitUntilValueChanged<T>(Func<T> valueGetter, IEqualityComparer<T>? equalityComparer = default, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return WaitUntilValueChanged(valueGetter, valueGetter.Invoke(), equalityComparer, timeout, cancellationToken);
        }

        internal sealed class WaitUntilValueChangedPromise<T> : WaitPromise<WaitUntilValueChangedPromise<T>>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static WaitUntilValueChangedPromise<T> Create(Func<T> func, T initialValue, IEqualityComparer<T>? equalityComparer, TimeSpan timeout, CancellationToken cancellationToken)
            {
                var promise = Rent();
                promise.value = initialValue;
                promise.func = func;
                promise.equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
                promise.OnInitialize(timeout, cancellationToken);
                return promise;
            }


            T value;
            Func<T>? func;
            IEqualityComparer<T>? equalityComparer;

            protected override bool CheckWaitingEnd() => !equalityComparer!.Equals(value, func!.Invoke());

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Reset()
            {
                base.Reset();

                value = default;
                func = default;
                equalityComparer = default;
            }
        }
    }
}
