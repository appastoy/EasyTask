using Xunit.Sdk;

namespace EasyTask.Tests;

[Collection(nameof(NoParallel))]
public class ETask_T_Tests
{
    [Fact]
    public async Task AsTask()
    {
        var value = await ETask.Run(() => GetValueWithDelay(2, 20)).AsTask();
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
        var value = await ETask.Run(() => GetValueWithDelay(2, 20)).AsValueTask();
        value.Should().Be(2);

        static async ETask<int> GetValueWithDelay(int v, int delayMilliSeconds)
        {
            await ETask.Delay(delayMilliSeconds);
            return v;
        }
    }

    [Fact]
    public void FromResult()
    {
        var value = 0;
        value = ETask.RunSynchronously(async () => await ETask.FromResult(1));
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
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
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
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var threadId = Environment.CurrentManagedThreadId;
            var runThreadId = threadId;

            runThreadId = await ETask.Run(() =>
                {
                    Thread.Sleep(1);
                    Thread.Sleep(1);
                    return Environment.CurrentManagedThreadId;
                })
                .ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(runThreadId);

            await ETask.SwitchToMainThread();

            runThreadId = await ETask.Run(Func2).ConfigureAwait(false);

            runThreadId.Should().NotBe(threadId);
            Environment.CurrentManagedThreadId.Should().Be(runThreadId);

            static async ETask<int> Func2()
            {
                await ETask.Yield();
                await ETask.Yield();
                return Environment.CurrentManagedThreadId;
            }
        }
    }

    [Fact]
    public void WhenAny()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var values = new bool[4];
            var (value, index) = await ETask.WhenAny(
                WaitAndSet(values, 1, 2),
                WaitAndSet(values, 10, 1),
                WaitAndSet(values, 100, 0),
                WaitAndSet(values, 1000, 3));
            value.Should().Be(100);
            index.Should().Be(2);

            values = new bool[4];
            (value, index) = await ETask.WhenAny(new List<ETask<int>>
            {
                WaitAndSet(values, 1000, 1),
                WaitAndSet(values, 100, 2),
                WaitAndSet(values, 10, 3),
                WaitAndSet(values, 1, 0)
            });
            value.Should().Be(1);
            index.Should().Be(3);

            static async ETask<int> WaitAndSet(bool[] values, int value, int index)
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
                return value;
            }
        }
    }

    [Fact]
    public void WhenAll()
    {
        ETask.RunSynchronously(Func);
        static async ETask Func()
        {
            var values = new bool[4];
            var results = await ETask.WhenAll(
                WaitAndSet(values, 1, 2),
                WaitAndSet(values, 10, 1),
                WaitAndSet(values, 100, 0),
                WaitAndSet(values, 1000, 3));
            results.Should().BeEquivalentTo(new[] {100,10,1,1000} );

            values = new bool[4];
            results = await ETask.WhenAll(new List<ETask<int>>
            {
                WaitAndSet(values, 1000, 1),
                WaitAndSet(values, 100, 2),
                WaitAndSet(values, 10, 3),
                WaitAndSet(values, 1, 0)
            });
            results.Should().BeEquivalentTo(new[] { 1, 1000, 100, 10 });

            static async ETask<int> WaitAndSet(bool[] values, int value, int index)
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
                return value;
            }
        }
    }

    [Fact]
    public void ContinueWith()
    {
        ETask.RunSynchronously(async () =>
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

            async ETask<string> Func()
            {
                await ETask.Yield();
                await ETask.Yield();
                return "a";
            }

            async ETask<string> Error()
            {
                await ETask.Yield();
                await ETask.Yield();
                throw new Exception();
            }

            async ETask<string> Cancel()
            {
                await ETask.Yield();
                await ETask.Yield();
                var token = new CancellationToken(true);
                token.ThrowIfCancellationRequested();
                return string.Empty;
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
            FuncETaskResultForget().Forget();
            await ETask.WaitForever();
        }
        async ETask<int> FuncETaskResultForget()
        {
            await ETask.Delay(20).ConfigureAwait(false);
            throw new Exception("abc");
        }
    }
}