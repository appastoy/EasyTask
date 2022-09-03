using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyTask;
public sealed class ETaskSynchronizationContext : SynchronizationContext
{
    public readonly int CreatedThreadId;
    List<(SendOrPostCallback call, object state)> processingPosts = new ();
    List<(SendOrPostCallback call, object state)> posts = new ();
    readonly List<Exception> exceptions = new();
    int @lock;


    public ETaskSynchronizationContext() : this(Environment.CurrentManagedThreadId)
    {
    }

    ETaskSynchronizationContext(int threadId)
    {
        CreatedThreadId = threadId;
    }

    public override SynchronizationContext CreateCopy()
    {
        return new ETaskSynchronizationContext(CreatedThreadId);
    }

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

    public override void Post(SendOrPostCallback d, object state)
    {
        using (new ScopeLock(this))
            posts.Add((d, state));
    }

    public void ProcessPosts()
    {
        ThrowIfCurrentThreadIsNotCreatedThread();
        if (posts.Count > 0)
            ProcessPostsCore();
    }

    public void ProcessPostsLoop()
    {
        ThrowIfCurrentThreadIsNotCreatedThread();
        while (posts.Count > 0)
            ProcessPostsCore();
    }

    void ThrowIfCurrentThreadIsNotCreatedThread([CallerMemberName] string methodName = "")
    {
        if (Environment.CurrentManagedThreadId != CreatedThreadId)
            throw new InvalidOperationException(
                        $"{methodName}() method should call on created thread. (created: {CreatedThreadId.ToString()}, {Environment.CurrentManagedThreadId.ToString()})");
    }

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

        public ScopeLock(ETaskSynchronizationContext context)
        {
            this.context = context;
            while (Interlocked.CompareExchange(ref context.@lock, 1, 0) != 0)
                Thread.Yield();
        }

        public void Dispose() => Volatile.Write(ref context.@lock, 0);
    }

    public readonly struct Scope : IDisposable
    {
        readonly SynchronizationContext? prevContext;
        readonly SynchronizationContext? prevMainThreadContext;

        public readonly ETaskSynchronizationContext Current;

        internal Scope(SynchronizationContext? prevContext)
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

    public static Scope CreateScope() => new(Current);
}
