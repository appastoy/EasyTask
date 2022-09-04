using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask;

/// <summary>
/// Custom synchronization context. If you set this to SynchronizationContext.Current, You should call ProcessPosts() or ProcessPostsLoop() method.
/// </summary>
public sealed class ETaskSynchronizationContext : SynchronizationContext
{
    public readonly int CreatedThreadId;
    List<(SendOrPostCallback call, object state)> processingPosts = new ();
    List<(SendOrPostCallback call, object state)> posts = new ();
    readonly List<Exception> exceptions = new();
    int @lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ETaskSynchronizationContext() : this(Environment.CurrentManagedThreadId)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ETaskSynchronizationContext(int threadId) => CreatedThreadId = threadId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override SynchronizationContext CreateCopy()
        => new ETaskSynchronizationContext(CreatedThreadId);

    /// <summary>
    /// Post a delegate and wait until delegate is invoked.
    /// </summary>
    /// <param name="d">delegate</param>
    /// <param name="state">delegate parameter</param>
    public override void Send(SendOrPostCallback d, object state)
    {
        if (Environment.CurrentManagedThreadId == CreatedThreadId)
        {
            d.Invoke(state);
        }
        else
        {
            List<(SendOrPostCallback call, object state)> capturedPosts;
            using (new ScopeLock(this))
            {
                capturedPosts = posts;
                posts.Add((d, state));
            }
            while (capturedPosts.Count > 0)
                Thread.Yield();
        }
    }

    /// <summary>
    /// Post a delegate. It invoked when ProcessPosts() or ProcessPostsLoop() is called.
    /// </summary>
    /// <param name="d">delegate</param>
    /// <param name="state">delegate parameter</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Post(SendOrPostCallback d, object state)
    {
        using var _ = new ScopeLock(this);
        posts.Add((d, state));
    }

    /// <summary>
    /// Process reserved posts. It process posts once.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessPosts()
    {
        ThrowIfCurrentThreadIsNotCreatedThread();
        if (posts.Count > 0)
            ProcessPostsCore();
    }

    /// <summary>
    /// Process reserved posts. It process posts repeatedly. (New posts may be reserved when posts are processed.)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessPostsLoop()
    {
        ThrowIfCurrentThreadIsNotCreatedThread();
        while (posts.Count > 0)
            ProcessPostsCore();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ThrowIfCurrentThreadIsNotCreatedThread([CallerMemberName] string methodName = "")
    {
        if (Environment.CurrentManagedThreadId != CreatedThreadId)
            throw new InvalidOperationException(
                        $"{methodName}() method should call on created thread. (created: {CreatedThreadId.ToString()}, {Environment.CurrentManagedThreadId.ToString()})");
    }

    [DebuggerHidden]
    void ProcessPostsCore()
    {
        try
        {
            var reservedPosts = SwapPosts();
            foreach (var (callback, state) in reservedPosts)
            {
                try
                {
                    callback.Invoke(state);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                    throw exceptions[0];
                else
                    throw new AggregateException(exceptions);
            }
        }
        finally
        {
            exceptions.Clear();
            processingPosts.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    List<(SendOrPostCallback callback, object state)> SwapPosts()
    {
        using var _ = new ScopeLock(this);

        var oldExecutions = processingPosts;
        processingPosts = posts;
        posts = oldExecutions;
            
        return processingPosts;
    }

    readonly struct ScopeLock : IDisposable
    {
        readonly ETaskSynchronizationContext context;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ScopeLock(ETaskSynchronizationContext context)
        {
            this.context = context;
            while (Interlocked.CompareExchange(ref context.@lock, 1, 0) != 0)
                Thread.Yield();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Volatile.Write(ref context.@lock, 0);
    }

    public readonly struct ContextScope : IDisposable
    {
        readonly SynchronizationContext? prevContext;
        readonly SynchronizationContext? prevMainThreadContext;

        public readonly ETaskSynchronizationContext Current;

        [DebuggerHidden]
        internal ContextScope(SynchronizationContext? prevContext)
        {
            this.prevContext = prevContext;
            prevMainThreadContext = ETask.MainThreadContext;

            Current = new ETaskSynchronizationContext();
            SetSynchronizationContext(Current);
            ETask.SetMainThreadContext(Current);
        }

        public void Dispose()
        {
            ETask.SetMainThreadContext(prevMainThreadContext);
            SetSynchronizationContext(prevContext);
        }
    }

    /// <summary>
    /// Create context scope. It provides a scope that SynchronizationContext.Current is a instance of ETaskSynchronizationContext.
    /// </summary>
    /// <returns>context scope</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ContextScope CreateScope() => new(Current);
}
