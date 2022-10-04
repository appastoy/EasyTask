namespace EasyTask.Tests;

[Collection(nameof(NoParallel))]
public class ETaskVoid_Tests
{
    private readonly ITestOutputHelper output;

    public ETaskVoid_Tests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void ReturnNormally()
    {
        ETask.RunSynchronously(Action);
        static async ETask Action()
        {
            int value = 0;
            Test().Forget();

            await ETask.WaitWhile(() => value == 0);

            value.Should().Be(1);

            async ETaskVoid Test()
            {
                await ETask.Delay(1);
                value = 1;
            }
        }
    }

    [Fact]
    public void ThrowException()
    {
        (new Action(() =>
        {
            bool isEnd = false;
            ETask.RunSynchronously(Action);
            async ETask Action()
            {
                Test().Forget();
                await ETask.WaitUntil(() => isEnd);
            }

            async ETaskVoid Test()
            {
                await ETask.Delay(1);
                try
                {
                    throw new Exception();
                }
                finally
                {
                    isEnd = true;
                }
            }
        }))
        .Should().Throw<Exception>();
    }

    [Fact]
    public void Canceled()
    {
        (new Action(() =>
        {
            bool isEnd = false;
            ETask.RunSynchronously(Action);
            async ETask Action()
            {
                Test(new CancellationToken(true)).Forget();
                await ETask.WaitUntil(() => isEnd);
            }

            async ETaskVoid Test(CancellationToken token)
            {
                await ETask.Delay(1, CancellationToken.None);
                try
                {
                    token.ThrowIfCancellationRequested();
                }
                finally
                {
                    isEnd = true;
                }
            }
        }))
        .Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void Forget()
    {
        new Action(() => ETask.RunSynchronously(FuncETask))
            .Should().Throw<Exception>()
            .And.Message.Should().Be("abc");
        async ETask FuncETask()
        {
            FuncETaskVoid().Forget();
            await ETask.WaitForever();
        }
        async ETaskVoid FuncETaskVoid()
        {
            await ETask.Delay(20).ConfigureAwait(false);
            throw new Exception("abc");
        }
    }
}
