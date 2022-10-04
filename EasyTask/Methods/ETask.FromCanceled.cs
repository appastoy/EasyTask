using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static readonly ETask CanceledTask = new ETask(
            new CanceledSource(new OperationCanceledException(new CancellationToken(true))), 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask FromCanceled(CancellationToken token)
        {
            if (token == CancellationToken.None)
                return CanceledTask;

            return new ETask(new CanceledSource(new OperationCanceledException(token)), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask FromCanceled(OperationCanceledException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new ETask(new CanceledSource(exception), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask<T> FromCanceled<T>(CancellationToken token)
        {
            if (token == CancellationToken.None)
                return CanceledSource<T>.CanceledTask;

            return new ETask<T>(new CanceledSource<T>(new OperationCanceledException(token)), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETask<T> FromCanceled<T>(OperationCanceledException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new ETask<T>(new CanceledSource<T>(exception), 0);
        }

        internal class CanceledSource : IETaskSource
        {
            public readonly OperationCanceledException Exception;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CanceledSource(OperationCanceledException exception)
                => Exception = exception;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult(short _) => throw Exception;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ETaskStatus GetStatus(short _) => ETaskStatus.Canceled;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action<object> continuation, object state, short token)
                => continuation.Invoke(state);
        }

        internal sealed class CanceledSource<T> : CanceledSource, IETaskSource<T>
        {
            public static readonly ETask<T> CanceledTask = new (
                new CanceledSource<T>(new OperationCanceledException(new CancellationToken(true))), 0);

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CanceledSource(OperationCanceledException exception)
                : base(exception)
            {

            }

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            new public T GetResult(short _) => throw Exception;
        }
    }
}
