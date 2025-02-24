using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PowerOfTenAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RecursionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "PT0102";

    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
        "Recursion should be avoided",
        "The method or constructor {0} could be called recursively: {1}",
        Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(SetupCallGraph);
    }

    private void SetupCallGraph(CompilationStartAnalysisContext compilationContext)
    {
        var state = new RecursionAnalyzerState(Rule);
        compilationContext.RegisterSemanticModelAction(state.AnalyzeSemanticModel);
        compilationContext.RegisterCompilationEndAction(state.OnCompilationEnd);
    }
}