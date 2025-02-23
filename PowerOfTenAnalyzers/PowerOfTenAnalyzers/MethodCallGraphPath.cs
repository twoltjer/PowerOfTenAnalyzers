using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace PowerOfTenAnalyzers;

public readonly struct MethodCallGraphPath : IEquatable<MethodCallGraphPath>
{
    public MethodCallGraphPath() : this(ImmutableList<MethodCallGraphEdge>.Empty)
    {
    }

    private MethodCallGraphPath(ImmutableList<MethodCallGraphEdge> edges)
    {
        Edges = edges;
    }
    
    public ImmutableList<MethodCallGraphEdge> Edges { get; }

    public bool TryAddEdge(MethodCallGraphEdge nextEdge, out MethodCallGraphPath? newPath, out bool hasRecursion)
    {
        Debug.Assert(!HasRecursion(Edges));
        if (Edges.Count > 0)
        {
            var lastEdgeEnd = Edges[Edges.Count - 1].CalledMethod;
            if (!nextEdge.CallingMethod.Equals(lastEdgeEnd, SymbolEqualityComparer.Default))
            {
                newPath = null;
                hasRecursion = false;
                return false;
            }
        }
        var newEdges = Edges.Add(nextEdge);
        newPath = new MethodCallGraphPath(newEdges);
        hasRecursion = HasRecursion(newEdges);
        return true;
    }

    private static bool HasRecursion(ImmutableList<MethodCallGraphEdge> edges)
    {
        if (edges.Count == 0)
            return false;
        
        var stack = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var firstEdge = edges[0];
        stack.Add(firstEdge.CallingMethod);
        foreach (var edge in edges)
        {
            Debug.Assert(stack.Contains(edge.CallingMethod));
            if (!stack.Add(edge.CalledMethod))
            {
                // Stack already had this method called; recursion found!
                return true;
            }
        }

        return false;
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