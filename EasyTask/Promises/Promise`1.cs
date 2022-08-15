using System;

namespace EasyTask.Promises
{
    internal class Promise<TPromise, T> : Promise<TPromise>, IPromise<T>
        where TPromise : Promise<TPromise, T>, new()
    {
#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
        T result;
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.

        new public ETask<T> Task => new ETask<T>(this, Token);

        new public T GetResult(short token)
        {
            try
            {
                ValidateToken(token);
                if (!IsCompletedInternal())
                    throw new InvalidOperationException("Task is not completed.");
                ThrowIfHasError();
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
        {
            base.BeforeReturn();
#pragma warning disable CS8601 // 가능한 null 참조 할당입니다.
            result = default;
#pragma warning restore CS8601 // 가능한 null 참조 할당입니다.
        }
    }
}
