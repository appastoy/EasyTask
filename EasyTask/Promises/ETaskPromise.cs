namespace EasyTask.Promises
{
    internal sealed class ETaskPromise : Promise<ETaskPromise>
    {

    }

    internal sealed class ETaskPromise<T> : Promise<ETaskPromise<T>, T>
    {

    }
}
