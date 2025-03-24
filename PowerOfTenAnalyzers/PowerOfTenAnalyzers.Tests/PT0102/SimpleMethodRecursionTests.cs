using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Xunit;

namespace PowerOfTenAnalyzers.Tests.PT0102;

public class SimpleMethodRecursionTests : SampleAnalyzerTests<RecursionAnalyzer>
{
    public SimpleMethodRecursionTests() : base("SimpleMethodRecursion.cs")
    {
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestSimpleMethodRecursion_ProducesDiagnostic()
    {
        var expected = AnalyzerVerifier<RecursionAnalyzer>.Diagnostic().WithLocation(13, 13).WithMessage("The method or constructor SimpleRecursiveMethod(int) could be called recursively: SimpleRecursiveMethod(int) -> SimpleRecursiveMethod(int)");
        await VerifyDiagnostics(expected);
    }
}