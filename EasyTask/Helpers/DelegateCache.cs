using System;
using System.Threading;

namespace EasyTask.Helpers
{
    internal static class DelegateCache
    {
        public static readonly Action<object?> InvokeAsActionObject = InvokeContinuation;
        public static readonly SendOrPostCallback InvokeAsSendOrPostCallback = InvokeSendPostCallback;
        public static readonly Action<Action> InvokeAsActionT = InvokeContinuationAction;

        public static void InvokeContinuation(object? obj) => (obj as Action)?.Invoke();
        public static void InvokeSendPostCallback(object obj) => ((Action)obj).Invoke();
        public static void InvokeContinuationAction(Action action) => action.Invoke();
    }
}
