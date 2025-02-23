using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PowerOfTenAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GotoStatementAnalyzer : GotoAnalyzerBase
{
    protected override SyntaxKind GetSyntaxKind()
    {
        return SyntaxKind.GotoStatement;
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GotoDefaultStatementAnalyzer : GotoAnalyzerBase
{
    protected override SyntaxKind GetSyntaxKind()
    {
        return SyntaxKind.GotoDefaultStatement;
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GotoCaseStatementAnalyzer : GotoAnalyzerBase
{
    protected override SyntaxKind GetSyntaxKind()
    {
        return SyntaxKind.GotoCaseStatement;
    }
}

public abstract class GotoAnalyzerBase : DiagnosticAnalyzer
{
    public const string DiagnosticId = "PT0101";

    private const string Category = "Naming";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Goto statements should be avoided", "Goto statements should be avoided, as per Rule 1 of the Power of 10 rules from the NASA/JPL Laboratory for Reliable Software", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSyntax, GetSyntaxKind());
    }

    protected abstract SyntaxKind GetSyntaxKind();

    private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        // This syntax kind is not okay with rule 1. Always return a diagnostic when it's found in user code.
        var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}