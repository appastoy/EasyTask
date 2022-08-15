using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        static readonly SendOrPostCallback InvokeOnPublish = OnPublish;
        
        public static event Action<Exception>? UnhandledExceptionHandler;

        internal static void PublishUnhandledException(ExceptionDispatchInfo exceptionDispatchInfo)
        {
            EnsureMainThreadSynchronizationContext();
            mainThreadSynchronizationContext?.Post(InvokeOnPublish, exceptionDispatchInfo);
        }

        static void OnPublish(object obj)
        {
            if (obj is ExceptionDispatchInfo info)
            {
                if (UnhandledExceptionHandler != null)
                    UnhandledExceptionHandler.Invoke(info.SourceException);
                else
                    info.Throw();
            }
        }
    }
}
