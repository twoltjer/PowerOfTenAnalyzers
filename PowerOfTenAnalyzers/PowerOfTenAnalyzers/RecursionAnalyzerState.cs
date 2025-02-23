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
        // Do DFS looking for recursion
        HashSet<MethodCallGraphPath> nonRecursivePaths = new HashSet<MethodCallGraphPath>();
        HashSet<MethodCallGraphPath> reportedPaths = new HashSet<MethodCallGraphPath>();
        foreach (var edge in _callGraph)
        {
            var oldPath = new MethodCallGraphPath();
            var result = oldPath.TryAddEdge(edge, out var newPath, out bool hasRecursion);
            Debug.Assert(result);
            if (!result)
                continue;

            if (hasRecursion)
            {
                bool alreadyReported = false;
                foreach (var reportedPath in reportedPaths)
                {
                    if (newPath!.Value.IsOrContainsPath(reportedPath))
                    {
                        alreadyReported = true;
                        break;
                    }
                }

                if (!alreadyReported)
                {
                    reportedPaths.Add(newPath!.Value);
                    {
                        foreach (var location in edge.Locations)
                        {
                            compilationEndContext.ReportDiagnostic(Diagnostic.Create(_rule, location,
                                MethodCallGraphPath.GetMethodName(newPath!.Value.Edges[0].CallingMethod), newPath.Value.GetStringRepresentation()));
                        }
                    }
                }
            }
            else
            {
                nonRecursivePaths.Add(newPath!.Value);
            }
        }

        while (nonRecursivePaths.Count > 0)
        {
            var nextNonRecursivePaths = new HashSet<MethodCallGraphPath>();
            foreach (var path in nonRecursivePaths)
            {
                foreach (var methodCall in _callGraph)
                {
                    if (path.TryAddEdge(methodCall, out var newPath, out bool hasRecursion))
                    {
                        if (hasRecursion)
                        {
                            bool alreadyReported = false;
                            foreach (var reportedPath in reportedPaths)
                            {
                                if (newPath!.Value.IsOrContainsPath(reportedPath))
                                {
                                    Debug.Assert(!newPath!.Value.Equals(reportedPath));
                                    alreadyReported = true;
                                    break;
                                }
                            }

                            if (!alreadyReported)
                                foreach (var location in methodCall.Locations)
                                {
                                    compilationEndContext.ReportDiagnostic(Diagnostic.Create(_rule, location,
                                        MethodCallGraphPath.GetMethodName(newPath!.Value.Edges[0].CallingMethod),
                                        newPath.Value.GetStringRepresentation()));
                                }
                        }
                        else
                        {
                            nextNonRecursivePaths.Add(newPath!.Value);
                        }
                    }
                }
            }

            nonRecursivePaths = nextNonRecursivePaths;
        }
    }
}