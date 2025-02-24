using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Xunit;

namespace PowerOfTenAnalyzers.Tests;

public class WhileLoopWithBreaksTests : SampleAnalyzerTests<LoopBoundsAnalyzer>
{
	public WhileLoopWithBreaksTests() : base("WhileLoopWithBreaks.cs")
	{
	}

	[Fact]
	public async Task TestWhileLoopWithBreaks()
	{
		var expected1 = AnalyzerVerifier<LoopBoundsAnalyzer>.Diagnostic().WithLocation(15, 5).WithMessage("Loop has sophisticated iteration logic, and such logic can cause loops to spin indefinitely. Avoid modifying the loop iterator, using a complex iteration expression, or breaking out of an intentionally infinite loop.");
		var expected2 = AnalyzerVerifier<LoopBoundsAnalyzer>.Diagnostic().WithLocation(33, 5).WithMessage("Loop has sophisticated iteration logic, and such logic can cause loops to spin indefinitely. Avoid modifying the loop iterator, using a complex iteration expression, or breaking out of an intentionally infinite loop.");
		await VerifyDiagnostics([expected1, expected2]);
	}
}