using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        static readonly SendOrPostCallback InvokeOnPublish = OnPublish;
        static readonly SendOrPostCallback InvokeOnThrow = OnThrow;

        /// <summary>
        /// <para>Unhandled exception in running task handler. If you set main thread context, Handler is called on main thread.</para>
        /// <para>If not, Handler is called on the thread where the task is running.</para>
        /// </summary>
        public static event Action<Exception>? UnhandledExceptionHandler;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void PublishUnhandledException(ExceptionDispatchInfo exceptionDispatchInfo)
        {
            if (mainThreadContext != null && mainThreadContext != SynchronizationContext.Current)
                mainThreadContext.Post(InvokeOnPublish, exceptionDispatchInfo);
            else
                OnPublish(exceptionDispatchInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void PublishUnhandledException(Exception exception)
        {
            if (mainThreadContext != null && mainThreadContext != SynchronizationContext.Current)
                mainThreadContext.Post(InvokeOnThrow, exception);
            else
                OnThrow(exception);
        }

        static void OnPublish(object obj)
        {
            var info = (ExceptionDispatchInfo)obj;
            if (UnhandledExceptionHandler != null)
                UnhandledExceptionHandler.Invoke(info.SourceException);
            else
                info.Throw();
        }

        static void OnThrow(object obj)
        {
            var exception = (Exception)obj;
            if (UnhandledExceptionHandler != null)
                UnhandledExceptionHandler.Invoke(exception);
            else
                throw exception;
        }
    }
}
