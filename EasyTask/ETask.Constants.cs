using System;
using System.Threading;

namespace EasyTask
{
    partial struct ETask
    {
        public static readonly ETask CompletedTask = new ETask();
    }
}
