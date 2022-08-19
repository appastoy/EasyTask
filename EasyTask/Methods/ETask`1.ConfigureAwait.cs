using EasyTask.Sources;

namespace EasyTask
{
    partial struct ETask<TResult>
    {
        public ConfigureAwaitable ConfigureAwait(bool captureContext)
        {
            if (!IsCompleted &&
                source is IConfigureAwaitable awaitable)
                awaitable.SetCaptureContext(captureContext);

            return new ConfigureAwaitable(in this);
        }

        public readonly struct ConfigureAwaitable
        {
            readonly ETask<TResult> task;

            public ConfigureAwaitable(in ETask<TResult> task) => this.task = task;

            public Awaiter GetAwaiter() => task.GetAwaiter();
        }
    }
}
