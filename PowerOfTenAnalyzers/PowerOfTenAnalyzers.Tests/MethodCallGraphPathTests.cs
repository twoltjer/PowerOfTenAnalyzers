using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace PowerOfTenAnalyzers.Tests;

public class MethodCallGraphPathTests
{
    [Fact]
    public void TestIsOrContainsPath_SameExactPath_ReturnsTrue()
    {
        var aMethod = CreateAndSetupMethodSymbol();
        var bMethod = CreateAndSetupMethodSymbol();
        var cMethod = CreateAndSetupMethodSymbol();

        var abcPath = CreatePath([aMethod, bMethod, cMethod]);
        Assert.True(abcPath.IsOrContainsPath(abcPath));
    }

    private static IMethodSymbol CreateAndSetupMethodSymbol()
    {
        var methodSymbol = Substitute.For<IMethodSymbol>();
        methodSymbol.Equals(Arg.Any<IMethodSymbol>(), SymbolEqualityComparer.Default)
            .Returns(x => ReferenceEquals(x[0], methodSymbol));
        return methodSymbol;
    }

    private static MethodCallGraphPath CreatePath(IReadOnlyList<IMethodSymbol> methods)
    {
        var current = new MethodCallGraphPath();
        for (int i = 1; i < methods.Count; i++)
        {
            Assert.True(current.TryAddEdge(new MethodCallGraphEdge(methods[i - 1], methods[i], [Location.None]),
                out var newPathOptional, out var hasRecursion));
            Assert.True(newPathOptional.HasValue);
            Assert.False(hasRecursion);
            current = newPathOptional.Value;
        }

        return current;
    }

    [Fact]
    public void TestIsOrContainsPath_HeadPath_ReturnsTrue()
    {
        var aMethod = CreateAndSetupMethodSymbol();
        var bMethod = CreateAndSetupMethodSymbol();
        var cMethod = CreateAndSetupMethodSymbol();
        var dMethod = CreateAndSetupMethodSymbol();
        var eMethod = CreateAndSetupMethodSymbol();

        var abcPath = CreatePath([aMethod, bMethod, cMethod]);
        var abcdePath = CreatePath([aMethod, bMethod, cMethod, dMethod, eMethod]);
        Assert.True(abcdePath.IsOrContainsPath(abcPath));
    }

    [Fact]
    public void TestIsOrContainsPath_TailPath_ReturnsTrue()
    {
        var aMethod = CreateAndSetupMethodSymbol();
        var bMethod = CreateAndSetupMethodSymbol();
        var cMethod = CreateAndSetupMethodSymbol();
        var dMethod = CreateAndSetupMethodSymbol();
        var eMethod = CreateAndSetupMethodSymbol();

        var cdePath = CreatePath([cMethod, dMethod, eMethod]);
        var abcdePath = CreatePath([aMethod, bMethod, cMethod, dMethod, eMethod]);
        Assert.True(abcdePath.IsOrContainsPath(cdePath));
    }

    [Fact]
    public void TestIsOrContainsPath_MiddlePath_ReturnsTrue()
    {
        var aMethod = CreateAndSetupMethodSymbol();
        var bMethod = CreateAndSetupMethodSymbol();
        var cMethod = CreateAndSetupMethodSymbol();
        var dMethod = CreateAndSetupMethodSymbol();
        var eMethod = CreateAndSetupMethodSymbol();

        var bcdPath = CreatePath([bMethod, cMethod, dMethod]);
        var abcdePath = CreatePath([aMethod, bMethod, cMethod, dMethod, eMethod]);
        Assert.True(abcdePath.IsOrContainsPath(bcdPath));
    }

    [Fact]
    public void TestIsOrContainsPath_LongerPath_ReturnsFalse()
    {
        var aMethod = CreateAndSetupMethodSymbol();
        var bMethod = CreateAndSetupMethodSymbol();
        var cMethod = CreateAndSetupMethodSymbol();
        var dMethod = CreateAndSetupMethodSymbol();
        var eMethod = CreateAndSetupMethodSymbol();

        var abcPath = CreatePath([aMethod, bMethod, cMethod]);
        var abcdePath = CreatePath([aMethod, bMethod, cMethod, dMethod, eMethod]);
        Assert.False(abcPath.IsOrContainsPath(abcdePath));
    }

    [Fact]
    public void TestIsOrContainsPath_JustNotFound_ReturnsFalse()
    {
        var aMethod = CreateAndSetupMethodSymbol();
        var bMethod = CreateAndSetupMethodSymbol();
        var cMethod = CreateAndSetupMethodSymbol();
        var dMethod = CreateAndSetupMethodSymbol();
        var eMethod = CreateAndSetupMethodSymbol();

        var abdePath = CreatePath([aMethod, bMethod, dMethod, eMethod]);
        var abcdePath = CreatePath([aMethod, bMethod, cMethod, dMethod, eMethod]);
        Assert.False(abcdePath.IsOrContainsPath(abdePath));
    }
}