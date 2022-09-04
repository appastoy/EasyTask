using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace EasyTask.Sources
{
    internal sealed class ExceptionHolder
    {
        readonly ExceptionDispatchInfo exceptionDispatchInfo;
        bool hasGetExceptionCalled;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExceptionHolder(ExceptionDispatchInfo exceptionDispatchInfo)
        {
            this.exceptionDispatchInfo = exceptionDispatchInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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