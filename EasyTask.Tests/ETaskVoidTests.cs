namespace EasyTask.Tests;
public class ETaskVoid_Tests
{
    [Fact]
    public void ReturnNormally()
    {
        ETask.RunSynchronously(Action);
        static void Action()
        {
            int value = 0;
            Test().Forget();

            while (value == 0)
                Thread.Yield();

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
            void Action()
            {
                Test().Forget();
                while (!isEnd)
                    Thread.Yield();
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
            void Action()
            {
                Test(new CancellationToken(true)).Forget();
                while (!isEnd)
                    Thread.Yield();
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
            while (true)
                await ETask.Yield();
        }
        async ETaskVoid FuncETaskVoid()
        {
            await ETask.Delay(20).ConfigureAwait(false);
            throw new Exception("abc");
        }
    }
}
