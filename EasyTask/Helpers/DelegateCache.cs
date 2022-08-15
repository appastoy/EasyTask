using System;
using System.Threading;

namespace EasyTask.Helpers
{
    internal static class DelegateCache
    {
        public static readonly Action<object?> InvokeAsActionObject = _InvokeObjectNullable;
        public static readonly SendOrPostCallback InvokeAsSendOrPostCallback = _InvokeObject;
        public static readonly Action<Action> InvokeAsActionT = _InvokeAction;
        public static readonly WaitCallback InvokeAsWaitCallback = _InvokeObjectNullable;

        public static void _InvokeObject(object obj) => ((Action)obj).Invoke();
        public static void _InvokeObjectNullable(object? obj) => (obj as Action)?.Invoke();
        public static void _InvokeAction(Action action) => action.Invoke();
    }
}
