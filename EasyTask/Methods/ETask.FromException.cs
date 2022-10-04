using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace EasyTask
{
    partial struct ETask
    {
        public static ETask FromException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (exception is OperationCanceledException oce)
                return FromCanceled(oce);

            return new ETask(new ExceptionSource(ExceptionDispatchInfo.Capture(exception)), 0);
        }

        public static ETask<T> FromException<T>(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (exception is OperationCanceledException oce)
                return FromCanceled<T>(oce);

            return new ETask<T>(new ExceptionSource<T>(ExceptionDispatchInfo.Capture(exception)), 0);
        }

        internal class ExceptionSource : IETaskSource
        {
            protected readonly ExceptionDispatchInfo exceptionDispatchInfo;
            bool hasGetResultOrGetException;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ExceptionSource(ExceptionDispatchInfo exceptionDispatchInfo)
            {
                this.exceptionDispatchInfo = exceptionDispatchInfo;
            }

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Exception GetException()
            {
                EnsureUseException();
                return exceptionDispatchInfo.SourceException;
            }

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult(short _)
            {
                EnsureUseException();
                exceptionDispatchInfo.Throw();
            }

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ETaskStatus GetStatus(short _) => ETaskStatus.Faulted;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action<object> continuation, object state, short token)
                => continuation?.Invoke(state);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected void EnsureUseException()
            {
                if (!hasGetResultOrGetException)
                {
                    hasGetResultOrGetException = true;
                    GC.SuppressFinalize(this);
                }
            }

            ~ExceptionSource()
            {
                if (!hasGetResultOrGetException)
                    PublishUnhandledException(exceptionDispatchInfo);
            }
        }

        internal sealed class ExceptionSource<T> : ExceptionSource, IETaskSource<T>
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ExceptionSource(ExceptionDispatchInfo exceptionDispatchInfo)
                : base(exceptionDispatchInfo) 
            {

            }

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            new public T GetResult(short _)
            {
                EnsureUseException();
                exceptionDispatchInfo.Throw();
                return default;
            }
        }
    }
}
