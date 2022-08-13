using System;
using System.Runtime.ExceptionServices;

namespace EasyTask.Sources
{
    internal sealed class ExceptionHolder
    {
        readonly ExceptionDispatchInfo exceptionDispatchInfo;
        bool hasGetExceptionCalled;

        public ExceptionHolder(ExceptionDispatchInfo exceptionDispatchInfo)
        {
            this.exceptionDispatchInfo = exceptionDispatchInfo;
        }

        public ExceptionDispatchInfo GetException()
        {
            if (!hasGetExceptionCalled)
            {
                GC.SuppressFinalize(this);
                hasGetExceptionCalled = true;
            }
            return exceptionDispatchInfo;
        }

        ~ExceptionHolder()
        {
            if (!hasGetExceptionCalled)
                ETask.PublishUnhandledException(exceptionDispatchInfo);
        }
    }
}