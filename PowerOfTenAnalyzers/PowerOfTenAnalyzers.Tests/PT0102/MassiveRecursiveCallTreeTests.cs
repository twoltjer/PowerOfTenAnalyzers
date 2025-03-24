using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing.XUnit;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using RandomRecursiveProjectGenerator;
using RandomRecursiveProjectGenerator.Tests;
using Xunit;

namespace PowerOfTenAnalyzers.Tests.PT0102;

public class MassiveRecursiveCallTreeTests
{
	[Fact]
	public async Task MassiveRecursiveCallTreeTest()
	{
		var test = new CSharpAnalyzerTest<RecursionAnalyzer, XUnitVerifier>();

		var generator = new MassiveRecursiveCallTreeSourceGenerator();

		var inputCompilation = MassiveRecursiveCallTreeSourceGeneratorTests.CreateEmptyCompilation();
		CSharpGeneratorDriver.Create(generator)
			.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out _);
		var generatedSources = outputCompilation.SyntaxTrees
			.Where(t => !inputCompilation.SyntaxTrees.Contains(t))
			.ToList();
		
		foreach (var tree in generatedSources)
			test.TestState.Sources.Add(await tree.GetTextAsync());

		await test.RunAsync();
	}
}