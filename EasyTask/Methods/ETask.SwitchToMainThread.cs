namespace EasyTask
{
    partial struct ETask
    {
        public static SwitchSynchronizationContextAwaitable SwitchToMainThread()
        {
            EnsureMainThreadSynchronizationContext();
            return new SwitchSynchronizationContextAwaitable(mainThreadSynchronizationContext);
        }
    }
}
