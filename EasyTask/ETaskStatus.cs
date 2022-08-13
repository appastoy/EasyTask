namespace EasyTask
{
    public enum ETaskStatus : byte
    {
        //
        // 요약:
        //     The operation has not yet completed.
        Pending = 0,
        //
        // 요약:
        //     The operation completed successfully.
        Succeeded = 1,
        //
        // 요약:
        //     The operation completed with an error.
        Faulted = 2,
        //
        // 요약:
        //     The operation completed due to cancellation.
        Canceled = 3
    }
}
