using Nito.AsyncEx;
using Xunit.Sdk;

namespace EasyTask.Tests;

public class ETaskTests
{
    [Fact]
    public async Task AsTask()
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

        var value = 1;
        await ETask.Run(() => SetValueWithDelay(2, 100)).AsTask();
        value.Should().Be(2);

        async ETask SetValueWithDelay(int v, int delayMilliSeconds)
        {
            await ETask.Delay(delayMilliSeconds);
            value = v;
        }
    }

    [Fact]
    public async Task AsValueTask()
    {
        var completedTask = ETask.CompletedTask.AsValueTask();
        completedTask.IsCompletedSuccessfully.Should().BeTrue();

        var canceledTask = ETask.CanceledTask.AsValueTask();
        canceledTask.IsCanceled.Should().BeTrue();

        var exceptionETask = ETask.FromException(new Exception());
        var exceptionTask = exceptionETask.AsValueTask();
        exceptionTask.IsFaulted.Should().BeTrue();

#pragma warning disable CS8602 // null 가능 참조에 대한 역참조입니다.
        exceptionTask.AsTask().Exception.InnerException.Should().Be(exceptionETask.Exception);
#pragma warning restore CS8602 // null 가능 참조에 대한 역참조입니다.

        var value = 1;
        await ETask.Run(() => SetValueWithDelay(2, 100)).AsValueTask();
        value.Should().Be(2);

        async ETask SetValueWithDelay(int v, int delayMilliSeconds)
        {
            await ETask.Delay(delayMilliSeconds);
            value = v;
        }
    }

    [Fact]
    public void Yield()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.Yield();
            threadId.Should().Be(Thread.CurrentThread.ManagedThreadId);
        }
    }

    [Fact]
    public void Delay()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            await ETask.Delay(1);
            Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);
        }
    }

    [Fact]
    public void SwitchToThreadPool()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            ETask.SetMainThreadSynchronizationContext(SynchronizationContext.Current);
            int threadId = Thread.CurrentThread.ManagedThreadId;

            await ETask.SwitchToThreadPool();

            var isThreadSame = Thread.CurrentThread.ManagedThreadId == threadId;

            await ETask.SwitchToMainThread();

            isThreadSame.Should().BeFalse();
        }
    }

    [Fact]
    public void SwitchSynchronizationContext()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            var currentContext = SynchronizationContext.Current;
            int threadId = Thread.CurrentThread.ManagedThreadId;

            await ETask.SwitchToThreadPool();

            var isThreadSame = Thread.CurrentThread.ManagedThreadId == threadId;

            await ETask.SwitchSynchronizationContext(currentContext);

            isThreadSame.Should().BeFalse();
            Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);
        }
    }

    [Fact]
    public void SwitchToMainThread()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            var mainThreadContext = SynchronizationContext.Current;
            ETask.SetMainThreadSynchronizationContext(mainThreadContext);
            int threadId = Thread.CurrentThread.ManagedThreadId;

            await ETask.SwitchToThreadPool();

            var isContextSame = SynchronizationContext.Current == mainThreadContext;

            await ETask.SwitchToMainThread();

            isContextSame.Should().BeFalse();
            threadId.Should().Be(Thread.CurrentThread.ManagedThreadId);
        }
    }

    [Fact]
    public async Task FromException()
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
    public async Task FromCanceled()
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

    [Fact]
    public async Task FromResult()
    {
        var context = SynchronizationContext.Current;
        var value = await ETask.FromResult(1);
        await ETask.SwitchSynchronizationContext(context);
        value.Should().Be(1);
    }

    [Fact]
    public void Run()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            ETask.SetMainThreadSynchronizationContext(SynchronizationContext.Current);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var runThreadId = threadId;
            
            await ETask.Run(() => runThreadId = Thread.CurrentThread.ManagedThreadId);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);

            await ETask.Run(Func2);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);

            ETask Func2()
            {
                runThreadId = Thread.CurrentThread.ManagedThreadId;
                return ETask.CompletedTask;
            }
        }
    }

    [Fact]
    public void Run_Without_CaptureContext()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            ETask.SetMainThreadSynchronizationContext(SynchronizationContext.Current);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var runThreadId = threadId;

            await ETask.Run(() => runThreadId = Thread.CurrentThread.ManagedThreadId, false);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().NotBe(threadId);

            await ETask.SwitchToMainThread();

            await ETask.Run(Func2, false);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().NotBe(threadId);

            ETask Func2()
            {
                runThreadId = Thread.CurrentThread.ManagedThreadId;
                return ETask.CompletedTask;
            }
        }
    }

    [Fact]
    public void WhenAny()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            var value = 0;
            await ETask.WhenAny(
                SetValueWithDelay(1, 1),
                SetValueWithDelay(2, 10000),
                SetValueWithDelay(3, 100000),
                SetValueWithDelay(4, 1000000));
            value.Should().Be(1);

            value = 0;
            await ETask.WhenAny(new List<ETask>
            {
                SetValueWithDelay(1, 1),
                SetValueWithDelay(2, 10000),
                SetValueWithDelay(3, 100000),
                SetValueWithDelay(4, 1000000)
            });
            value.Should().Be(1);

            async ETask SetValueWithDelay(int v, int delayMilliSeconds)
            {
                await ETask.Delay(delayMilliSeconds);
                value = v;
            }
        }
    }

    [Fact]
    public void WhenAll()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            var value = 0;
            await ETask.WhenAll(
                SetValueWithDelay(1, 1),
                SetValueWithDelay(2, 20),
                SetValueWithDelay(3, 40),
                SetValueWithDelay(4, 60));
            value.Should().Be(4);

            value = 0;
            await ETask.WhenAll(new List<ETask>
            {
                SetValueWithDelay(1, 1),
                SetValueWithDelay(2, 20),
                SetValueWithDelay(3, 40),
                SetValueWithDelay(4, 60)
            });
            value.Should().Be(4);

            async ETask SetValueWithDelay(int v, int delayMilliSeconds)
            {
                await ETask.Delay(delayMilliSeconds);
                value = v;
            }
        }
    }
}