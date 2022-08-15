using Nito.AsyncEx;
using Xunit.Sdk;

namespace EasyTask.Tests;

public class ETaskTests
{
    [Fact]
    public void ETask_Should_Convert_Task()
    {
        var completedTask = ETask.CompletedTask.AsTask();
        completedTask.IsCompletedSuccessfully.Should().BeTrue();

        var canceledTask = ETask.CanceledTask.AsTask();
        canceledTask.IsCanceled.Should().BeTrue();

        var exceptionETask = ETask.FromException(new Exception());
        var exceptionTask = exceptionETask.AsTask();
        exceptionTask.IsFaulted.Should().BeTrue();

#pragma warning disable CS8602 // null 가능 참조에 대한 역참조입니다.
        exceptionTask.Exception.InnerException.Should().Be(exceptionETask.Exception);
#pragma warning restore CS8602 // null 가능 참조에 대한 역참조입니다.
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