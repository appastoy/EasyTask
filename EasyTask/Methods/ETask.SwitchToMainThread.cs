namespace EasyTask
{
    partial struct ETask
    {
        public static SwitchSynchronizationContextAwaitable SwitchToMainThread()
        {
            ValidateMainThreadSynchronizationContext();
            return new SwitchSynchronizationContextAwaitable(mainThreadSynchronizationContext);
        }
    }
}
