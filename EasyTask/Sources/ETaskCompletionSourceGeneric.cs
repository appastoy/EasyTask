namespace EasyTask.Sources
{
    public abstract class ETaskCompletionSourceGeneric<TSource>
        : ETaskCompletionSourceBase<TSource>, IETaskSource
        where TSource : ETaskCompletionSourceGeneric<TSource>, new()
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
