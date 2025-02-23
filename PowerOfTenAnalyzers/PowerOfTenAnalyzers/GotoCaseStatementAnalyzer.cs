using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PowerOfTenAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GotoCaseStatementAnalyzer : GotoAnalyzerBase
{
    protected override SyntaxKind GetSyntaxKind()
    {
        return SyntaxKind.GotoCaseStatement;
    }
}