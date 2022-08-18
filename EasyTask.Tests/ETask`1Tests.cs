using Nito.AsyncEx;
using Xunit.Sdk;

namespace EasyTask.Tests;

public class ETask_T_Tests
{
    [Fact]
    public async Task AsTask()
    {
        var value = await ETask.Run(() => GetValueWithDelay(2, 100)).AsTask();
        value.Should().Be(2);

        async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
        {
            await ETask.Delay(delayMilliSeconds);
            return v;
        }
    }

    [Fact]
    public async Task AsValueTask()
    {
        var value = await ETask.Run(() => GetValueWithDelay(2, 100)).AsValueTask();
        value.Should().Be(2);

        async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
        {
            await ETask.Delay(delayMilliSeconds);
            return v;
        }
    }

    [Fact]
    public async Task FromResult()
    {
        var context = SynchronizationContext.Current;
        var value = await ETask.FromResult(1);

#pragma warning disable CS8604
        await ETask.SwitchSynchronizationContext(context);
#pragma warning restore CS8604

        value.Should().Be(1);
    }

    [Fact]
    public async Task FromException()
    {
        try
        {
            await ETask.FromException<int>(new Exception("abc"));
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
            await ETask.FromCanceled<int>(new OperationCanceledException());
            throw new XunitException("CanceledTask should throw OperationCanceledException.");
        }
        catch (OperationCanceledException)
        {
        }
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

            runThreadId = await ETask.Run(() => Thread.CurrentThread.ManagedThreadId);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);

            runThreadId = await ETask.Run(Func2);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);

            async ETask<int> Func2()
            {
                await ETask.Yield();
                return Thread.CurrentThread.ManagedThreadId;
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

            runThreadId = await ETask.Run(() =>
                {
                    Thread.Sleep(1);
                    return Thread.CurrentThread.ManagedThreadId;
                })
                .ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().NotBe(threadId);

            await ETask.SwitchToMainThread();

            await ETask.Run(Func2).ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Thread.CurrentThread.ManagedThreadId.Should().NotBe(threadId);

            async ETask<int> Func2()
            {
                await ETask.Yield();
                return Thread.CurrentThread.ManagedThreadId;
            }
        }
    }

    [Fact]
    public void WhenAny()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            var (value, index) = await ETask.WhenAny(
                GetValueWithDelay(1, 1000),
                GetValueWithDelay(2, 10000),
                GetValueWithDelay(3, 1),
                GetValueWithDelay(4, 1000000));
            value.Should().Be(3);
            index.Should().Be(2);

            (value, index) =await ETask.WhenAny(new List<ETask<int>>
            {
                GetValueWithDelay(1, 10000),
                GetValueWithDelay(2, 1),
                GetValueWithDelay(3, 100000),
                GetValueWithDelay(4, 1000000)
            });
            value.Should().Be(2);
            index.Should().Be(1);

            async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
            {
                await ETask.Delay(delayMilliSeconds);
                return v;
            }
        }
    }

    [Fact]
    public void WhenAll()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            var values = await ETask.WhenAll(
                GetValueWithDelay(1, 1),
                GetValueWithDelay(2, 20),
                GetValueWithDelay(3, 40),
                GetValueWithDelay(4, 60));
            values.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });

            values = await ETask.WhenAll(new List<ETask<int>>
            {
                GetValueWithDelay(4, 1),
                GetValueWithDelay(3, 20),
                GetValueWithDelay(2, 40),
                GetValueWithDelay(1, 60)
            });
            values.Should().BeEquivalentTo(new[] {4,3,2,1});

            async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
            {
                await ETask.Delay(delayMilliSeconds);
                return v;
            }
        }
    }
}