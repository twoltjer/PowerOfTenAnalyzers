using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace PowerOfTenAnalyzers;

public readonly struct RecursionAnalyzerState
{
    private readonly DiagnosticDescriptor _rule;
    private readonly HashSet<MethodCallGraphEdge> _callGraph;

    public RecursionAnalyzerState(DiagnosticDescriptor rule)
    {
        _rule = rule;
        _callGraph = new HashSet<MethodCallGraphEdge>();
    }

    public void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
    {
        var semanticModel = context.SemanticModel;
        var root = semanticModel.SyntaxTree.GetRoot(context.CancellationToken);
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();
        var objectCreations = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var caller =
                semanticModel.GetEnclosingSymbol(invocation.SpanStart, context.CancellationToken) as IMethodSymbol;
            var callee = semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
            if (caller == null || callee == null)
                continue;
            var found = false;
            lock (_callGraph)
            {
                foreach (var call in _callGraph)
                {
                    if (call.CalledMethod.Equals(callee, SymbolEqualityComparer.Default) &&
                        call.CallingMethod.Equals(caller, SymbolEqualityComparer.Default))
                    {
                        found = true;
                        call.Locations.Add(invocation.GetLocation());
                        break;
                    }
                }

                if (!found)
                {
                    _callGraph.Add(new MethodCallGraphEdge(caller, callee, [invocation.GetLocation()]));
                }
            }
        }

        foreach (var creation in objectCreations)
        {
            var caller =
                semanticModel.GetEnclosingSymbol(creation.SpanStart, context.CancellationToken) as IMethodSymbol;
            var callee = semanticModel.GetSymbolInfo(creation, context.CancellationToken).Symbol as IMethodSymbol;
            if (caller == null || callee == null)
                continue;
            var found = false;
            lock (_callGraph)
            {
                foreach (var call in _callGraph)
                {
                    if (call.CalledMethod.Equals(callee, SymbolEqualityComparer.Default) &&
                        call.CallingMethod.Equals(caller, SymbolEqualityComparer.Default))
                    {
                        found = true;
                        call.Locations.Add(creation.GetLocation());
                        break;
                    }
                }

                if (!found)
                {
                    _callGraph.Add(new MethodCallGraphEdge(caller, callee, [creation.GetLocation()]));
                }
            }
        }
    }

    public void OnCompilationEnd(CompilationAnalysisContext compilationEndContext)
    {
        var callGraphOrganizedByStartingEdge = new CallGraphOrganizedByStartingEdge();
        callGraphOrganizedByStartingEdge.AddEdges(_callGraph);

        var recursivePaths = new HashSet<MethodCallGraphPath>();
        CreateSingleEdgePaths(recursivePaths, callGraphOrganizedByStartingEdge, out var incompletePaths);
        while (incompletePaths.Count > 0)
        {
            incompletePaths = ExtendPaths(recursivePaths, incompletePaths, callGraphOrganizedByStartingEdge);
        }

        ReportDiagnostics(recursivePaths, compilationEndContext);
    }

    private void ReportDiagnostics(
        IReadOnlyCollection<MethodCallGraphPath> recursivePaths,
        CompilationAnalysisContext compilationEndContext
        )
    {
        foreach (var path in recursivePaths)
        {
            foreach (var location in path.Edges[0].Locations)
            {
                compilationEndContext.ReportDiagnostic(
                    Diagnostic.Create(
                        _rule,
                        location,
                        MethodCallGraphPath.GetMethodName(path.Edges[0].CallingMethod),
                        path.GetStringRepresentation()
                        )
                    );
            }
        }
    }

    private void CreateSingleEdgePaths(
        ISet<MethodCallGraphPath> recursivePaths,
        CallGraphOrganizedByStartingEdge callGraphOrganizedByStartingEdge,
        out HashSet<MethodCallGraphPath> incompletePaths
        )
    {
        incompletePaths = new HashSet<MethodCallGraphPath>();
        foreach (var edge in _callGraph)
        {
            var path = new MethodCallGraphPath().AddEdge(edge);
            if (path.HasRecursion)
            {
                recursivePaths.Add(path);
            }
            else
            {
                var tail = path.Tail!;
                var isIncomplete = callGraphOrganizedByStartingEdge.GetEdgesFromMethod(tail).Count > 0;
                if (isIncomplete)
                {
                    incompletePaths.Add(path);
                }
            }
        }
    }

    private HashSet<MethodCallGraphPath> ExtendPaths(ISet<MethodCallGraphPath> recursivePaths, ISet<MethodCallGraphPath> incompletePaths, CallGraphOrganizedByStartingEdge callGraphOrganizedByStartingEdge)
    {
        var nextIncompletePaths = new HashSet<MethodCallGraphPath>();
        foreach (var path in incompletePaths)
        {
            var tail = path.Tail!;
            var nextEdges = callGraphOrganizedByStartingEdge.GetEdgesFromMethod(tail);
            foreach (var edge in nextEdges)
            {
                var newPath = path.AddEdge(edge);
                if (newPath.HasRecursion)
                {
                    // Check if dropping the head is still recursive. If so, a we can disregard adding this recursion to the recursive paths
                    var newPathEdges = newPath.Edges;
                    Debug.Assert(newPathEdges.Count > 1);
                    var headlessPath = new MethodCallGraphPath();
                    for (int i = 1; i < newPathEdges.Count; i++)
                    {
                        headlessPath = headlessPath.AddEdge(newPathEdges[i]);
                    }

                    if (!headlessPath.HasRecursion)
                    {
                        recursivePaths.Add(newPath);
                    }
                }
                else
                {
                    tail = newPath.Tail!;
                    var isIncomplete = callGraphOrganizedByStartingEdge.GetEdgesFromMethod(tail).Count > 0;
                    if (isIncomplete)
                    {
                        nextIncompletePaths.Add(newPath);
                    }
                }
            }
        }

        return nextIncompletePaths;
    }
}

public readonly struct CallGraphOrganizedByStartingEdge
{
    private readonly Dictionary<IMethodSymbol, HashSet<MethodCallGraphEdge>> _callGraphOrganized = new Dictionary<IMethodSymbol, HashSet<MethodCallGraphEdge>>(SymbolEqualityComparer.Default);

    public CallGraphOrganizedByStartingEdge()
    {
    }

    public void AddEdges(IReadOnlyCollection<MethodCallGraphEdge> edges)
    {
        foreach (var edge in edges)
        {
            AddEdge(edge);
        }
    }

    private void AddEdge(MethodCallGraphEdge edge)
    {
        var caller = edge.CallingMethod;
        if (!_callGraphOrganized.TryGetValue(caller, out var set))
            set = _callGraphOrganized[caller] = new HashSet<MethodCallGraphEdge>();
        set.Add(edge);
    }

    public IReadOnlyCollection<MethodCallGraphEdge> GetEdgesFromMethod(IMethodSymbol? method)
    {
        Debug.Assert(method != null);
        if (method == null)
            return Array.Empty<MethodCallGraphEdge>();
        
        if (_callGraphOrganized.TryGetValue(method, out var set))
            return set;

        return Array.Empty<MethodCallGraphEdge>();
    }
}