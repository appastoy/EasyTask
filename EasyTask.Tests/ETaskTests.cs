using Nito.AsyncEx;
using Xunit.Sdk;

namespace EasyTask.Tests;

public class ETaskTests
{
    [Fact]
    public void ETask_Should_Convert_Task()
    {
        var completedTask = ETask.CompletedTask.ToTask();
        completedTask.IsCompletedSuccessfully.Should().BeTrue();

        var canceledTask = ETask.CanceledTask.ToTask();
        canceledTask.IsCanceled.Should().BeTrue();

        var exceptionETask = ETask.FromException(new Exception());
        var exceptionTask = exceptionETask.ToTask();
        exceptionTask.IsFaulted.Should().BeTrue();
        exceptionTask.Exception.InnerException.Should().Be(exceptionETask.Exception);
    }

    [Fact]
    public void Yield_Should_Not_Change_Thread()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.Yield();
            int threadIdAfterYield = Thread.CurrentThread.ManagedThreadId;
            threadId.Should().Be(threadIdAfterYield);
        }
    }

    [Fact]
    public void Delay_Should_Not_Change_Thread()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.Delay(1);
            int threadIdAfterYield = Thread.CurrentThread.ManagedThreadId;
            threadId.Should().Be(threadIdAfterYield);
        }
    }

    [Fact]
    public void SwitchToThreadPool_Should_Change_Thread()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.SwitchToThreadPool();
            int threadIdAfterYield = Thread.CurrentThread.ManagedThreadId;
            threadId.Should().NotBe(threadIdAfterYield);
        }
    }

    [Fact]
    public void SwitchToMainThread_Should_Change_To_Main_Thread()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            var mainThreadContext = SynchronizationContext.Current;
            ETask.SetMainThreadSynchronizationContext(mainThreadContext);
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.SwitchToThreadPool();
            SynchronizationContext.Current.Should().NotBe(mainThreadContext);
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