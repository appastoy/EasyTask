using Xunit.Abstractions;
using Xunit.Sdk;

namespace EasyTask.Tests;

[Collection(nameof(NoParallel))]
public class ETask_Tests
{
    readonly ITestOutputHelper output;

    public ETask_Tests(ITestOutputHelper output)
    {
        this.output = output;
    }

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

#pragma warning disable CS8602
        exceptionTask.Exception.InnerException.Should().Be(exceptionETask.Exception);
#pragma warning restore CS8602

        var value = 1;
        await ETask.Run(() => SetValueWithDelay(2, 20)).AsTask();
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

#pragma warning disable CS8602
        exceptionTask.AsTask().Exception.InnerException.Should().Be(exceptionETask.Exception);
#pragma warning restore CS8602

        var value = 1;
        await ETask.Run(() => SetValueWithDelay(2, 20)).AsValueTask();
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
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            int threadId = Environment.CurrentManagedThreadId;
            await ETask.Yield();
            threadId.Should().Be(Environment.CurrentManagedThreadId);
        }
    }

    [Fact]
    public void Delay()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            int threadId = Environment.CurrentManagedThreadId;
            await ETask.Delay(1);
            Environment.CurrentManagedThreadId.Should().Be(threadId);
        }
    }

    [Fact]
    public void SwitchToThreadPool()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            int threadId = Environment.CurrentManagedThreadId;

            await ETask.SwitchToThreadPool();

            var isThreadSame = Environment.CurrentManagedThreadId == threadId;

            await ETask.SwitchToMainThread();

            isThreadSame.Should().BeFalse();
        }
    }

    [Fact]
    public void SwitchSynchronizationContext()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            SynchronizationContext? currentContext = SynchronizationContext.Current ?? throw new ArgumentNullException("SynchronizationContext.Current");
            int threadId = Environment.CurrentManagedThreadId;

            await ETask.SwitchToThreadPool();

            var isThreadSame = Environment.CurrentManagedThreadId == threadId;

            await ETask.SwitchSynchronizationContext(currentContext);

            isThreadSame.Should().BeFalse();
            Environment.CurrentManagedThreadId.Should().Be(threadId);
        }
    }

    [Fact]
    public void SwitchToMainThread()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var context = SynchronizationContext.Current;
            int threadId = Environment.CurrentManagedThreadId;

            await ETask.SwitchToThreadPool();

            var isContextSame = SynchronizationContext.Current == context;

            await ETask.SwitchToMainThread();

            isContextSame.Should().BeFalse();
            threadId.Should().Be(Environment.CurrentManagedThreadId);
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

        try
        {
            await ETask.FromCanceled(new OperationCanceledException());
            throw new XunitException("CanceledTask should throw OperationCanceledException.");
        }
        catch (OperationCanceledException)
        {
        }

        try
        {
            await ETask.FromCanceled(new CancellationToken(true));
            throw new XunitException("CanceledTask should throw OperationCanceledException.");
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Fact]
    public void Run()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var threadId = Environment.CurrentManagedThreadId;
            var runThreadId = threadId;
            
            await ETask.Run(() => runThreadId = Environment.CurrentManagedThreadId);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(threadId);

            await ETask.Run(Func2);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(threadId);

            ETask Func2()
            {
                runThreadId = Environment.CurrentManagedThreadId;
                return ETask.CompletedTask;
            }
        }
    }

    [Fact]
    public void RunSynchronously()
    {
        {
            int etaskInvoked = 0;
            ETask.RunSynchronously(async () =>
            {
                etaskInvoked = 1;
                await ETask.Yield();
                etaskInvoked = 3;
            });
            etaskInvoked.Should().Be(3);
        }

        {
            var value = ETask.RunSynchronously(async () =>
            {
                string value = "a";
                await ETask.Yield();
                value = "c";
                return value;
            });
            value.Should().Be("c");
        }
    }

    [Fact]
    public void RunTaskSynchronously()
    {
        {
            int etaskInvoked = 0;
            ETask.RunTaskSynchronously(async () =>
            {
                etaskInvoked = 1;
                await Task.Yield();
                etaskInvoked = 3;
            });
            etaskInvoked.Should().Be(3);
        }

        {
            var value = ETask.RunTaskSynchronously(async () =>
            {
                string value = "a";
                await Task.Yield();
                value = "c";
                return value;
            });
            value.Should().Be("c");
        }
    }

    [Fact]
    public void RunValueTaskSynchronously()
    {
        {
            int etaskInvoked = 0;
            ETask.RunValueTaskSynchronously(async () =>
            {
                etaskInvoked = 1;
                await Task.Yield();
                etaskInvoked = 3;
            });
            etaskInvoked.Should().Be(3);
        }

        {
            var value = ETask.RunValueTaskSynchronously(async () =>
            {
                string value = "a";
                await Task.Yield();
                value = "c";
                return value;
            });
            value.Should().Be("c");
        }
    }

    [Fact]
    public void ConfigureAwait()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var threadId = Environment.CurrentManagedThreadId;
            var runThreadId = threadId;

            await ETask.Run(new Action(() =>
                {
                    Thread.Sleep(1);
                    Thread.Sleep(1);
                    runThreadId = Environment.CurrentManagedThreadId;
                }))
                .ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(runThreadId);

            await ETask.SwitchToMainThread();

            await ETask.Run(Func2).ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(runThreadId);

            async ETask Func2()
            {
                await ETask.Yield();
                await ETask.Delay(10);
                runThreadId = Environment.CurrentManagedThreadId;
            }
        }
    }

    [Fact]
    public void WhenAny()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var result = 0;
            await ETask.WhenAny(
                WaitAndSet(1, 2),
                WaitAndSet(10000, 1),
                WaitAndSet(100000, 4),
                WaitAndSet(1000000, 3));
            result.Should().Be(2);

            result = 0;
            await ETask.WhenAny(new List<ETask> 
            {
                WaitAndSet(100000, 2),
                WaitAndSet(10000, 1),
                WaitAndSet(1, 4),
                WaitAndSet(1000000, 3)
            });
            result.Should().Be(4);

            async ETask WaitAndSet(int waitMilliseconds, int value)
            {
                await ETask.Delay(waitMilliseconds);
                result = value;
            }
        }
    }

    [Fact]
    public void WhenAll()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var result = false;
            var values = new bool[4];
            await ETask.WhenAll(
                WaitAndSet(values, 2),
                WaitAndSet(values, 1),
                WaitAndSet(values, 0),
                WaitAndSet(values, 3));
            result.Should().BeTrue();

            result = false;
            values = new bool[4];
            await ETask.WhenAll(new List<ETask>
            {
                WaitAndSet(values, 1),
                WaitAndSet(values, 2),
                WaitAndSet(values, 3),
                WaitAndSet(values, 0)
            });
            result.Should().BeTrue();

            async ETask WaitAndSet(bool[] values, int index)
            {
                if (index == 0)
                {
                    values![0] = true;
                }
                else
                {
                    await ETask.WaitUntil(() => values![index - 1]);
                    values![index] = true;
                }
                if (values.All(v => v))
                    result = true;
            }
        }
    }

    [Fact]
    public void ContinueWith()
    {
        ETask.RunSynchronously(async () =>
        {
            var list = new List<string>();
            var threadId = Environment.CurrentManagedThreadId;
            var threadIdInContinueWith = 0;

            {
                await Action().ContinueWith(task =>
                {
                    list.Add("b");
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                });
                threadIdInContinueWith.Should().Be(threadId);
                list.Should().BeEquivalentTo(new[] { "a", "b" });
            }

            {
                list.Clear();
                await Action().ContinueWith((task, state) =>
                {
                    list.Add((string)state);
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                }, "c");
                threadIdInContinueWith.Should().Be(threadId);
                list.Should().BeEquivalentTo(new[] { "a", "c" });
            }

            {
                list.Clear();
                var result = await Action().ContinueWith(task =>
                {
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                    return list[0];
                });
                threadIdInContinueWith.Should().Be(threadId);
                result.Should().Be("a");
            }

            {
                list.Clear();
                var result = await Action().ContinueWith((task, state) =>
                {
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                    return list[0] + (string)state;
                }, "d");
                threadIdInContinueWith.Should().Be(threadId);
                result.Should().Be("ad");
            }
            
            {
                bool isFault = false;
                await Error().ContinueWith(task => isFault = task.IsFaulted);
                isFault.Should().BeTrue();
            }

            {
                bool isCanceled = false;
                await Cancel().ContinueWith(task => isCanceled = task.IsCanceled);
                isCanceled.Should().BeTrue();
            }

            async ETask Action()
            {
                await ETask.Yield();
                await ETask.Yield();
                list.Add("a");
            }

            async ETask Error()
            {
                await ETask.Yield();
                await ETask.Yield();
                throw new Exception();
                
            }

            async ETask Cancel()
            {
                await ETask.Yield();
                await ETask.Yield();
                var token = new CancellationToken(true);
                token.ThrowIfCancellationRequested();
            }
        });
    }

    [Fact]
    public void Forget()
    {
        new Action(() => ETask.RunSynchronously(FuncETask))
            .Should().Throw<Exception>()
            .And.Message.Should().Be("abc");
        async ETask FuncETask()
        {
            FuncETaskForget().Forget();
            while (true)
                await ETask.Yield();
        }
        async ETask FuncETaskForget()
        {
            await ETask.Delay(20).ConfigureAwait(false);
            throw new Exception("abc");
        }
    }
}