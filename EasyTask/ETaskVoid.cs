using EasyTask.CompilerServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EasyTask
{
    [StructLayout(LayoutKind.Auto, Pack = 1)]
    [AsyncMethodBuilder(typeof(ETaskVoidMethodBuilder))]
    public readonly struct ETaskVoid
    {
    }
}
