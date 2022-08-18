using EasyTask.Sources;

namespace EasyTask
{
    partial struct ETask
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
            readonly ETask task;

            public ConfigureAwaitable(in ETask task) => this.task = task;

            public Awaiter GetAwaiter() => task.GetAwaiter();
        }
    }
}
