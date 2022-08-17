namespace EasyTask.Sources
{
    public abstract class ETaskCompletionSourceGeneric<TSource, T>
        : ETaskCompletionSourceBase<TSource>, IETaskSource<T>
        where TSource : ETaskCompletionSourceGeneric<TSource, T>, new()
    {
#pragma warning disable CS8618
        T result;
#pragma warning restore CS8618

        public ETask<T> Task => new ETask<T>(this, Token);

        public T GetResult(short token)
        {
            try
            {
                GetResultInternal(token);
                return result;
            }
            finally
            {
                Return();
            }
        }

        public void TrySetResult(T result)
        {
            if (IsCompletedFirst())
            {
                this.result = result;
                InvokeContinuation();
            }
        }

        protected override void BeforeReturn()
#pragma warning disable CS8601
            => result = default;
#pragma warning restore CS8601
    }
}
