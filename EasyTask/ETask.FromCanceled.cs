using System;
using System.Threading;
using EasyTask.Sources;

namespace EasyTask
{
    partial struct ETask
    {
        public static ETask FromCanceled(CancellationToken token)
        {
            if (token == CancellationToken.None)
                return CanceledTask;

            return new ETask(new CanceledSource(new OperationCanceledException(token)), 0);
        }

        public static ETask FromCanceled(OperationCanceledException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new ETask(new CanceledSource(exception), 0);
        }

        internal sealed class CanceledSource : IETaskSource
        {
            readonly OperationCanceledException exception;
            
            public CanceledSource(OperationCanceledException exception)
                => this.exception = exception;
            
            public void GetResult(short _) => throw exception;
            
            public ETaskStatus GetStatus(short _) => ETaskStatus.Canceled;
            
            public void OnCompleted(Action<object?>? continuation, object? state, short token)
                => continuation?.Invoke(state);
        }
    }
}
