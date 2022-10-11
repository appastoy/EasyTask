# EasyTask

[![.NET Test](https://github.com/appastoy/EasyTask/actions/workflows/dotnet_test.yml/badge.svg?branch=develop)](https://github.com/appastoy/EasyTask/actions/workflows/dotnet_test.yml)
[![.NET Publish](https://github.com/appastoy/EasyTask/actions/workflows/dotnet_publish.yml/badge.svg?branch=master)](https://github.com/appastoy/EasyTask/actions/workflows/dotnet_publish.yml)
[![Nuget](https://img.shields.io/nuget/v/appastoy.EasyTask?label=nuget&logo=nuget)](https://www.nuget.org/packages/appastoy.EasyTask/)

This project is for learning purpose to better understand the "Task". 
However, I made this for practical use. So I considered optimization and maintainability.
I didn't fully test all areas, but most methods were tested through unit tests. 
This project is strongly inspired by the [UniTask project](https://github.com/Cysharp/UniTask).
But the purpose of this project is different from UniTask that it is not for Unity3d first. 
It is based on netstandard2.1 and is expected to be used by many dotnet projects.

# Installation
To install with NuGet, just install the `appastoy.EasyTask`:
```powershell
Install-Package appastoy.EasyTask
```
<br>

# Quick Start
### Run on thread pool
```csharp
// Run action on thread pool.
ETask actionTask = ETask.Run(() => Console.WriteLine("HelloWorld")); 

// Run func on thread pool.
ETask<int> funcTask = ETask.Run(() => { return 1; });
```
<br>

### Yield and Delay
```csharp
// Yield to other thread.
await ETask.Yield();

// Await delay. (Milliseconds or TimeSpan)
await ETask.Delay(1200);
```
<br>

### Default and Creation
```csharp
// Await default completed task.
await ETask.CompletedTask;

// Create completed task with result.
await ETask.FromResult("abc");

// Await default canceled task.
await ETask.CanceledTask;

// Create canceled task with OperationCanceledException.
await ETask.FromCanceled(new OperationCanceledException());

// Create fault task with exception.
await ETask.FromException(new Exception());
```
<br>

### Forget
```csharp
// Forget provides a way to catch an exception without await.
// And it is a syntax sugar for ignoring compile warnings.

HeavyWork().Forget();

async ETask HeavyWork()
{
    await ETask.Delay(10000000);
    throw new Exception("It's too heavy.");
}
```
<br>

### ToTask, ToValueTask

```csharp
async ETask DelayedWork()
{
    await ETask.Delay(1000);
    Console.WriteLine("Work");
}
Task task = DelayedWork().ToTask();
ValueTask task = DelayedWork().ToValueTask();
```
<br>

### Continue With
```csharp
// Continue with action after ETask.
await DelayedHello().ContinueWith(task => Console.WriteLine("World1"));
// Output: HelloWorld1

// Continue with action with state after ETask.
await DelayedHello().ContinueWith((task, state) => Console.WriteLine(state), "World1-1");
// Output: HelloWorld1-2

// Continue with action after ETask<string>.
await DelayedHelloReturn().ContinueWith(task => Console.WriteLine($"{task.Result}World2"));
// Output: HelloWorld2

// Continue with func after ETask.
var world3 = await DelayedHello().ContinueWith(task => "World3");
Console.WriteLine(world3);
// Output: HelloWorld3

// Continue with func with state after ETask.
var world3 = await DelayedHello().ContinueWith((task, state) => state, "World3-2");
Console.WriteLine(world3);
// Output: HelloWorld3-2

// Continue with func after ETask<string>.
var world4 = await DelayedHelloReturn().ContinueWith(task => $"{task.Result}World4");
Console.WriteLine(world4);
// Output: HelloWorld4

async ETask DelayedHello()
{
    await ETask.Delay(10);
    Conosole.WriteLine("Hello");
}

async ETask<string> DelayedHelloReturn()
{
    await ETask.Delay(10);
    return "Hello";
}

```
<br>

### Configure Await
```csharp
// Capture current context. (default)
int threadIdBeforeAwait = Environment.CurrentManagedThreadId;
await ETask.Delay(100);
int threadIdAfterAwait = Environment.CurrentManagedThreadId;

// Basically, If SynchronizationContext.Current is not null, 
// the thread after await is the same as the thread before await.
// All methods of ETask are same behavior. (ConfigureAwait(true) is default.)
Assert.Equal(threadIdBeforeAwait, threadIdAfterAwait);


// No capture currrent context.
threadIdBeforeAwait = Environment.CurrentManagedThreadId;
await ETask.Delay(200).ConfigureAwait(false);
threadIdAfterAwait = Environment.CurrentManagedThreadId;

// If you use ConfigureAwait(false) method, 
// It does NOT guarantee that the thread after await the same as the thread before await. (It could be the same)
// All methods of ETask are same behavior.
// But, performance is better than when you don't use.
Assert.NotEqaul(threadIdBeforeAwait, threadIdAfterAwait);
```
<br>

### Run Synchronously
```csharp
// Run synchronously with synchronization context posting garenteed.

// Run ETask action.
int etaskInvoked = 0;
ETask.RunSynchronously(async () =>
{
    etaskInvoked = 1;
    await ETask.Yield();
    etaskInvoked = 3;
});
etaskInvoked.Should().Be(3);

// Run ETask<T> func.
var value = ETask.RunSynchronously(async () =>
{
    string value = "a";
    await ETask.Yield();
    value = "c";
    return value;
});
value.Should().Be("c");
```
<br>

### Switch
```csharp
// You can set main thread context once at first of your code.
var entryContext = SynchronizationContext.Current;
ETask.SetMainThreadContext(entryContext);

var entryThreadId = Environment.CurrentManagedThreadId;

// Switch to thread pool.
await ETask.SwitchToThreadPool();

var threadPoolThreadId = Environment.CurrentManagedThreadId;
Assert.NotEqual(entryThreadId, threadPoolThreadId);

// Switch to main thread.
await ETask.SwitchToMainThread();

var mainThreadId = Environment.CurrentManagedThreadId;
Assert.NotEqual(threadPoolThreadId, mainThreadId);
Assert.AreEqual(entryThreadId, mainThreadId);

// Switch to thread pool again.
await ETask.SwitchToThreadPool();

threadPoolThreadId = Environment.CurrentManagedThreadId;

// Switch specific synchronization context.
await ETask.SwitchSynchronizationContext(entryContext);

var contextThreadId = Environment.CurrentManagedThreadId;
Assert.AreEqual(entryThreadId, contextThreadId);
```
<br>

### When all and any
```csharp
// WhenAll is await when all tasks are completed.
// This will end up waiting for a 100 millisecond delay.
await ETask.WhenAll(
    ETask.Delay(1),
    ETask.Delay(10),
    ETask.Delay(100)
);

// WhenAny is await when any task is completed.
// This will end up waiting for a 1 millisecond delay.
await ETask.WhenAny(
    ETask.Delay(1),
    ETask.Delay(10),
    ETask.Delay(100)
);
```
<br>

### Use ETaskVoid
```csharp
// When you don't need to await, You can consider using ETaskVoid.

// If you use "async void", You will miss exception that is thrown after await something.
async void AsyncVoid()
{
    await Task.Delay(100);
    throw new Exception(); // this will disappear.
}

// This method never throw exception.
AsyncVoid();


// But if you use "async ETaskVoid", You can catch any exception on the main thread.
async ETaskVoid AsyncETaskVoid()
{
    await ETask.Delay(100);
    throw new Exception();
}

// You can set main thread context once at first of your code.
ETask.SetMainThreadContext(SynchronizationContext.Current);

// It will throw exception after 100ms delayed.
// Forget method is syntax sugar for ignoring compile warning. Actually, Forget method body is empty.
AsyncETaskVoid().Forget();
```

<br>

# Overview
### ValueTask based
ETask is based on ValueTask for performance purpose. It has an IValueTaskSource implementation internally.
```csharp
interface IETaskSource : IValueTaskSource { ... }
class ETaskSource : IETaskSource { ... }

struct ETask
{
    ...
    IETaskSource source;
    ...
}
```
<br>

### Using delegate caches
ETask doesn't use lambda expression internally for avoid generate garbage. <br>
If lambda has captured the value(not constant), It causes heap allocation. ([C# Does Lambda => generate garbage?](https://stackoverflow.com/questions/7133013/c-sharp-does-lambda-generate-garbage))<br>
So it uses static delegate caches instead of lamda expression.
```csharp
// The style of using lambda style.
// This is easy and simple. But it generates garbage each use.
var obj = "abc";
ThreadPool.QueueUserWorkItem(action => action.Invoke($"{obj}"), Console.WriteLine);

// The style of using static delegate cache.
// This is more complicate than lambda. But it doesn't generate garbage anything.
// And also there is no cost to convert static method to action parameter each use.
static Action<string> OnWorkDelegate = OnWork;
static void OnWork(string obj) => Console.WriteLine(obj);
ThreadPool.QueueUserWorkItem(OnWorkDelegate, "abc");
```
<br>

### Using various pools
ETask uses various pools internally for avoid heap allocation and garbage collection.

- The subclasse of __PoolItem\<T>__ is an item and the pool itself.
    ```csharp
    public abstract class PoolItem<TItem> : IDisposable where ...
    {
        // Use one-way linked list pool.
        public static TItem Rent() { ... }
        public void Return() { ... }
        ...
    }
  
    public sealed class ETaskSource : PoolItem<ETaskSource> { ... }

    var itemRented = ETaskSource.Rent();
    // Use item.
    // ...
    itemRented.Return();
    ```

- __ListPool__ provides a temporary list of the requested size.
    ```csharp
    interface IListPoolItem { void Return(); }
  
    class ListItem<T> : PoolItem<ListItem<T>>, IReadOnlyList<T>, IListPoolItem
    {
        public static ListItem<T> Rent(int capacity) { ... }
    }

    var itemRented = ListItem<int>.Rent(10);
    // Use item.
    // ...
    itemRented.Return();
    ```

- __TuplePool__ provides a temporary tuple. It is the same with C# Tuple.
    ```csharp
    class FieldTuple<T1, T2> : PoolItem<FieldTuple<T1, T2>>, IDisposable
    {
        public T1 _1;
        public T2 _2;
    }
    ...
  
    static class TuplePool
    {
        public static FieldTuple<T1, T2> Rent<T1, T2>(T1 _1, T2 _2)
        {
            var tuple = FieldTuple<T1, T2>.Rent();
            tuple._1 = _1;
            tuple._2 = _2;
            return tuple;
        }
        ...
    }

    var itemRented = TuplePool.Rent(123, "abc");
    // Use item.
    // ...
    itemRented.Return();
  ```

<br>

