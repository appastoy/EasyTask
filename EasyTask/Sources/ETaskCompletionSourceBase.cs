namespace EasyTask.Sources
{
    public abstract class ETaskCompletionSourceBase<TSource>
        : ETaskSourceBase<TSource>, IETaskCompletionSource
        where TSource : ETaskCompletionSourceBase<TSource>, new()
    {
        public ETask Task => new ETask(this, Token);

        public void GetResult(short token)
        {
            try
            {
                GetResultInternal(token);
            }
            finally
            {
                Return();
            }
        }

        public void TrySetResult()
        {
            if (IsCompletedFirst())
                InvokeContinuation();
        }
    }
}
