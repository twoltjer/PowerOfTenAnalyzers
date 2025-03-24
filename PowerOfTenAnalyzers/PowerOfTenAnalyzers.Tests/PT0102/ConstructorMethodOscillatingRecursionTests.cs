using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Xunit;

namespace PowerOfTenAnalyzers.Tests.PT0102;

public class ConstructorMethodOscillatingRecursionTests : SampleAnalyzerTests<RecursionAnalyzer>
{
    public ConstructorMethodOscillatingRecursionTests() : base("ConstructorMethodOscillatingRecursion.cs")
    {
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestSimpleMethodRecursion_ProducesDiagnostic()
    {
        var expectedOne = AnalyzerVerifier<RecursionAnalyzer>.Diagnostic().WithLocation(9, 9).WithMessage("The method or constructor ConstructorMethodOscillatingRecursion.ctor() could be called recursively: ConstructorMethodOscillatingRecursion.ctor() -> Method() -> ConstructorMethodOscillatingRecursion.ctor()");
        var expectedTwo = AnalyzerVerifier<RecursionAnalyzer>.Diagnostic().WithLocation(14, 20).WithMessage("The method or constructor Method() could be called recursively: Method() -> ConstructorMethodOscillatingRecursion.ctor() -> Method()");
        var expectedThree = AnalyzerVerifier<RecursionAnalyzer>.Diagnostic().WithLocation(15, 21).WithMessage("The method or constructor Method() could be called recursively: Method() -> ConstructorMethodOscillatingRecursion.ctor() -> Method()");

        await VerifyDiagnostics([expectedOne, expectedTwo, expectedThree]);
    } 
}