#pragma warning disable CS8618
#pragma warning disable CS8601

namespace EasyTask.Sources
{
    public abstract class ETaskCompletionSourceBase<TSource, T>
        : ETaskSourceBase<TSource>, IETaskCompletionSource<T>
        where TSource : ETaskCompletionSourceBase<TSource, T>, new()
    {

        T result;

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

        protected override void Reset()
        {
            base.Reset();
            result = default;
        }
    }
}
