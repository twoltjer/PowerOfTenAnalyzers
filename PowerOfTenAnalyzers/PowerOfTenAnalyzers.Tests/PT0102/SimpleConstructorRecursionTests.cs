using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Xunit;

namespace PowerOfTenAnalyzers.Tests.PT0102;

public class SimpleConstructorRecursionTests : SampleAnalyzerTests<RecursionAnalyzer>
{
    public SimpleConstructorRecursionTests() : base("SimpleConstructorRecursion.cs")
    {
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestSimpleMethodRecursion_ProducesDiagnostic()
    {
        var expected = AnalyzerVerifier<RecursionAnalyzer>.Diagnostic().WithLocation(10, 25).WithMessage("The method or constructor SimpleConstructorRecursion.ctor(int) could be called recursively: SimpleConstructorRecursion.ctor(int) -> SimpleConstructorRecursion.ctor(int)");
        await VerifyDiagnostics(expected);
    }
}