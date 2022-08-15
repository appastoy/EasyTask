using System;
using System.Threading;
using EasyTask.Sources;

namespace EasyTask
{
    partial struct ETask
    {
        public static readonly ETask CanceledTask = new ETask(
            new CanceledSource(new OperationCanceledException(new CancellationToken(true))), 0);

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
            public readonly OperationCanceledException Exception;
            
            public CanceledSource(OperationCanceledException exception)
                => Exception = exception;
            
            public void GetResult(short _) => throw Exception;
            
            public ETaskStatus GetStatus(short _) => ETaskStatus.Canceled;
            
            public void OnCompleted(Action<object?>? continuation, object? state, short token)
                => continuation?.Invoke(state);
        }
    }
}
