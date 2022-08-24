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

        static async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
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

        static async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
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
            var threadId = Environment.CurrentManagedThreadId;
            var runThreadId = threadId;

            runThreadId = await ETask.Run(() => Environment.CurrentManagedThreadId);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(threadId);

            runThreadId = await ETask.Run(Func2);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(threadId);

            static async ETask<int> Func2()
            {
                await ETask.Yield();
                return Environment.CurrentManagedThreadId;
            }
        }
    }

    [Fact]
    public void ConfigureAwait()
    {
        AsyncContext.Run(Func);
        static async Task Func()
        {
            ETask.SetMainThreadSynchronizationContext(SynchronizationContext.Current);
            var threadId = Environment.CurrentManagedThreadId;
            var runThreadId = threadId;

            runThreadId = await ETask.Run(() =>
                {
                    Thread.Sleep(1);
                    return Environment.CurrentManagedThreadId;
                })
                .ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().NotBe(threadId);

            await ETask.SwitchToMainThread();

            await ETask.Run(Func2).ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().NotBe(threadId);

            static async ETask<int> Func2()
            {
                await ETask.Yield();
                return Environment.CurrentManagedThreadId;
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

            static async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
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

            static async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
            {
                await ETask.Delay(delayMilliSeconds);
                return v;
            }
        }
    }

    [Fact]
    public void ContinueWith()
    {
        AsyncContext.Run(async () =>
        {
            var testString = string.Empty;
            var threadId = Environment.CurrentManagedThreadId;
            var threadIdInContinueWith = 0;

            {
                await Func().ContinueWith(task =>
                {
                    testString = task.Result + "b";
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                });
                threadIdInContinueWith.Should().Be(threadId);
                testString.Should().Be("ab");
            }

            {
                await Func().ContinueWith((task, state) =>
                {
                    testString = task.Result + (string)state;
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                }, "c");
                threadIdInContinueWith.Should().Be(threadId);
                testString.Should().Be("ac");
            }

            {
                var result = await Func().ContinueWith(task =>
                {
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                    return task.Result + "d";
                });
                threadIdInContinueWith.Should().Be(threadId);
                result.Should().Be("ad");
            }

            {
                var result = await Func().ContinueWith((task, state) =>
                {
                    threadIdInContinueWith = Environment.CurrentManagedThreadId;
                    return task.Result + (string)state;
                }, "f");
                threadIdInContinueWith.Should().Be(threadId);
                result.Should().Be("af");
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

            ETask<string> Func()
            {
                return ETask.FromResult("a");
            }

            async ETask<string> Error()
            {
                await ETask.Yield();
                throw new Exception();
            }

            async ETask<string> Cancel()
            {
                await ETask.Yield();
                var token = new CancellationToken(true);
                token.ThrowIfCancellationRequested();
                return string.Empty;
            }
        });
    }
}