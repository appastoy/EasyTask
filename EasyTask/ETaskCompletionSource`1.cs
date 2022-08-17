using EasyTask.Sources;

namespace EasyTask
{
    public sealed class ETaskCompletionSource<T>
        : ETaskCompletionSourceGeneric<ETaskCompletionSource<T>, T>
    { }
}
