using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Xunit;

namespace PowerOfTenAnalyzers.Tests;

public class CrossFileRecursionTests : SampleAnalyzerTests<RecursionAnalyzer>
{
    public CrossFileRecursionTests() : base("CrossFileRecursionA.cs", "CrossFileRecursionB.cs")
    {
    }

    [Fact]
    public async Task TestRecursionBetweenTwoFiles()
    {
        var expectedA = AnalyzerVerifier<RecursionAnalyzer>.Diagnostic().WithLocation("/0/Test1.cs", 7, 9).WithMessage("The method or constructor Method() could be called recursively: Method() -> Method() -> Method()");
        var expectedB = AnalyzerVerifier<RecursionAnalyzer>.Diagnostic().WithLocation("/0/Test0.cs", 7, 9).WithMessage("The method or constructor Method() could be called recursively: Method() -> Method() -> Method()");
        await VerifyDiagnostics([expectedA, expectedB]);
    }
}