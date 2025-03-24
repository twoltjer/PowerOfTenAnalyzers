using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Xunit;

namespace PowerOfTenAnalyzers.Tests.PT0101;

public class GotoStatementAnalyzerTests : SampleAnalyzerTests<GotoStatementAnalyzer>
{
    public GotoStatementAnalyzerTests() : base("TestGotoClass.cs")
    {
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGotoStatementSyntax_ReportsDiagnostic_Always()
    {
        var expected = AnalyzerVerifier<GotoStatementAnalyzer>.Diagnostic()
            .WithLocation(7, 9);
        await VerifyDiagnostics(expected);
    }
}

public class GotoCaseStatementAnalyzerTests : SampleAnalyzerTests<GotoCaseStatementAnalyzer>
{
    public GotoCaseStatementAnalyzerTests() : base("TestGotoClass.cs")
    {
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGotoCaseStatementSyntax_ReportsDiagnostic_Always()
    {
        var expected = AnalyzerVerifier<GotoCaseStatementAnalyzer>.Diagnostic()
            .WithLocation(18, 17);
        await VerifyDiagnostics(expected);
    }
}

public class GotoDefaultStatementAnalyzerTests : SampleAnalyzerTests<GotoDefaultStatementAnalyzer>
{
    public GotoDefaultStatementAnalyzerTests() : base("TestGotoClass.cs")
    {
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGotoDefaultStatementSyntax_ReportsDiagnostic_Always()
    {
        var expected = AnalyzerVerifier<GotoDefaultStatementAnalyzer>.Diagnostic()
            .WithLocation(20, 17);
        await VerifyDiagnostics(expected);
    }
}