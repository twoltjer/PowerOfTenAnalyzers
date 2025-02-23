using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PowerOfTenAnalyzers;

public readonly struct MethodCallGraphEdge : IEquatable<MethodCallGraphEdge>
{
    public MethodCallGraphEdge(IMethodSymbol callingMethod, IMethodSymbol calledMethod, HashSet<Location> locations)
    {
        CallingMethod = callingMethod;
        CalledMethod = calledMethod;
        Locations = locations;
    }

    public IMethodSymbol CallingMethod { get; }
    public IMethodSymbol CalledMethod { get; }
    public ISet<Location> Locations { get; }

    public bool Equals(MethodCallGraphEdge other)
    {
        if (!CallingMethod.Equals(other.CallingMethod, SymbolEqualityComparer.Default))
            return false;
        
        if (!CalledMethod.Equals(other.CalledMethod, SymbolEqualityComparer.Default))
            return false;
        
        if (Locations.Count != other.Locations.Count)
            return false;
        
        foreach (var location in Locations)
            if (!other.Locations.Contains(location))
                return false;
            
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is MethodCallGraphEdge other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            const int leftPrime = 17;
            const int rightPrime = 23;
            int hash = leftPrime;
            hash = hash * rightPrime + SymbolEqualityComparer.Default.GetHashCode(CalledMethod);
            hash = hash * rightPrime + SymbolEqualityComparer.Default.GetHashCode(CallingMethod);
            hash = hash * rightPrime + Locations.Count;
            foreach (var location in Locations.OrderBy(l => l.GetHashCode()))
                hash = hash * rightPrime + location.GetHashCode();
            return hash;
        }
    }
}