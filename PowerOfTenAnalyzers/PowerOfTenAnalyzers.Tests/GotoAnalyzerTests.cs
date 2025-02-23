using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Xunit;
namespace PowerOfTenAnalyzers.Tests;

public class GotoAnalyzerTests
{
    private readonly string _gotoClassCode;

    public GotoAnalyzerTests()
    {
        const string relativePath = "../../../../../PowerOfTenAnalyzers.Sample/TestGotoClass.cs";
        var runningAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        var path = Path.Combine(runningAssemblyLocation, relativePath);
        _gotoClassCode = File.ReadAllText(path);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGotoStatementSyntax_ReportsDiagnostic_Always()
    {
        var expected = AnalyzerVerifier<GotoStatementAnalyzer>.Diagnostic()
            .WithLocation(7, 9);
        await AnalyzerVerifier<GotoStatementAnalyzer>.VerifyAnalyzerAsync(_gotoClassCode, expected);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGotoDefaultStatementSyntax_ReportsDiagnostic_Always()
    {
        var expected = AnalyzerVerifier<GotoDefaultStatementAnalyzer>.Diagnostic()
            .WithLocation(20, 17);
        await AnalyzerVerifier<GotoDefaultStatementAnalyzer>.VerifyAnalyzerAsync(_gotoClassCode, expected);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGotoCaseStatementSyntax_ReportsDiagnostic_Always()
    {
        var expected = AnalyzerVerifier<GotoCaseStatementAnalyzer>.Diagnostic()
            .WithLocation(18, 17);
        await AnalyzerVerifier<GotoCaseStatementAnalyzer>.VerifyAnalyzerAsync(_gotoClassCode, expected);
    }
}