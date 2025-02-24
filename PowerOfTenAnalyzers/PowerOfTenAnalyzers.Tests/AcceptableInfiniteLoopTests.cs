using System.Threading.Tasks;
using Xunit;

namespace PowerOfTenAnalyzers.Tests;

public class AcceptableInfiniteLoopTests : SampleAnalyzerTests<LoopBoundsAnalyzer>
{
	public AcceptableInfiniteLoopTests() : base("AcceptableInfiniteLoops.cs")
	{
	}

	[Fact]
	[Trait("Category", "Unit")]
	public async Task TestProducesNoDiagnostics()
	{
		await VerifyDiagnostics([]);
	}
}