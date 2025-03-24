using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace PowerOfTenAnalyzers;

public readonly struct MethodCallGraphPath : IEquatable<MethodCallGraphPath>
{
    public MethodCallGraphPath() : this(ImmutableList<MethodCallGraphEdge>.Empty, ImmutableList<IMethodSymbol>.Empty, ImmutableHashSet<IMethodSymbol>.Empty, false)
    {
    }

    private MethodCallGraphPath(ImmutableList<MethodCallGraphEdge> edges, ImmutableList<IMethodSymbol> stack, ImmutableHashSet<IMethodSymbol> stackSet, bool hasRecursion)
    {
        Edges = edges;
        HasRecursion = hasRecursion;
        _stack = stack;
        _stackSet = stackSet;
    }

    public ImmutableList<MethodCallGraphEdge> Edges { get; }

    private readonly ImmutableList<IMethodSymbol> _stack;

    private readonly ImmutableHashSet<IMethodSymbol> _stackSet;
    public bool HasRecursion { get; }
    
    public IMethodSymbol? Tail => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;
    public IMethodSymbol? Head => _stack.Count > 0 ? _stack[0] : null;

    public MethodCallGraphPath AddEdge(MethodCallGraphEdge nextEdge)
    {
        var newEdges = Edges.Add(nextEdge);
        var newStack = _stack;
        var newStackSet = _stackSet;
        if (Edges.Count == 0)
        {
            newStack = newStack.Add(nextEdge.CallingMethod);
            newStackSet = newStackSet.Add(nextEdge.CallingMethod);
        }
        var previousCount = newStackSet.Count;
        newStack = newStack.Add(nextEdge.CalledMethod);
        newStackSet = newStackSet.Add(nextEdge.CalledMethod);
        var newCount = newStackSet.Count;
        var hasRecursion = previousCount == newCount;
        var newPath = new MethodCallGraphPath(newEdges, newStack, newStackSet, hasRecursion);
        return newPath;
    }

    public bool Equals(MethodCallGraphPath other)
    {
        return Edges.Equals(other.Edges);
    }

    public override bool Equals(object? obj)
    {
        return obj is MethodCallGraphPath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Edges.GetHashCode();
    }

    public string GetStringRepresentation()
    {
        if (Edges.Count == 0)
            return string.Empty;
        
        var stack = new List<string>(Edges.Count + 1);
        stack.Add(GetMethodName(Edges[0].CallingMethod));
        foreach (var edge in Edges)
            stack.Add(GetMethodName(edge.CalledMethod));
        return string.Join(" -> ", stack);
    }

    public static string GetMethodName(IMethodSymbol methodSymbol)
    {
        var name = methodSymbol.Name;
        if (name == ".ctor")
        {
            name = $"{methodSymbol.ContainingType.Name}.ctor";
        }
        return $"{name}({string.Join(", ", methodSymbol.Parameters)})";
    }

    public bool IsOrContainsPath(MethodCallGraphPath other)
    {
        Debug.Assert(other.Edges.Count > 0);
        Debug.Assert(Edges.Count > 0);
        for (int i = 0; i < Edges.Count - other.Edges.Count + 1; i++)
        {
            var match = true;
            for (int j = 0; j < other.Edges.Count; j++)
            {
                var thisEdge = Edges[i + j];
                var otherEdge = other.Edges[j];
                if (!thisEdge.Equals(otherEdge))
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return true;
        }

        return false;
    }
}