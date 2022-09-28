global using Xunit;
global using Xunit.Abstractions;
global using FluentAssertions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace EasyTask.Tests;

[CollectionDefinition(nameof(NoParallel), DisableParallelization = true)]
public class NoParallel { }