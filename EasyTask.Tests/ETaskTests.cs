using Nito.AsyncEx;
using Xunit.Sdk;

namespace EasyTask.Tests;

public class ETaskTests
{
    [Fact]
    public async Task Yield_Should_Not_Change_Thread()
    {
        await AsyncContext.Run(Func);
        static async ETask Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.Yield();
            int threadIdAfterYield = Thread.CurrentThread.ManagedThreadId;
            threadId.Should().Be(threadIdAfterYield);
        }
    }

    [Fact]
    public async Task Delay_Should_Not_Change_Thread()
    {
        await AsyncContext.Run(Func);
        static async ETask Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.Delay(1);
            int threadIdAfterYield = Thread.CurrentThread.ManagedThreadId;
            threadId.Should().Be(threadIdAfterYield);
        }
    }

    [Fact]
    public async Task SwitchToThreadPool_Should_Change_Thread()
    {
        await AsyncContext.Run(Func);
        static async ETask Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.SwitchToThreadPool();
            int threadIdAfterYield = Thread.CurrentThread.ManagedThreadId;
            threadId.Should().NotBe(threadIdAfterYield);
        }
    }

    [Fact]
    public async Task SwitchToMainThread_Should_Change_To_Main_Thread()
    {
        await AsyncContext.Run(Func);
        static async ETask Func()
        {
            ETask.SetMainThreadSynchronizationContext(SynchronizationContext.Current);
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.SwitchToThreadPool();
            await ETask.SwitchToMainThread();
            int threadIdAfterYield = Thread.CurrentThread.ManagedThreadId;
            threadId.Should().Be(threadIdAfterYield);
        }
    }

    [Fact]
    public async Task FromException_Should_Throw_Exception()
    {
        try
        {
            await ETask.FromException(new Exception("abc"));
        }
        catch (Exception ex)
        {
            ex.Message.Should().Be("abc");
        }
    }

    [Fact]
    public async Task FromCanceled_Should_Throw_OperationCaceledException()
    {
        try
        {
            await ETask.CanceledTask;
            throw new XunitException("CanceledTask should throw OperationCanceledException.");
        }
        catch (OperationCanceledException)
        {
        }
    }
}