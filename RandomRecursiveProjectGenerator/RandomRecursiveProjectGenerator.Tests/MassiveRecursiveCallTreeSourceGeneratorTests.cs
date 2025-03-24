using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace RandomRecursiveProjectGenerator.Tests;

public class MassiveRecursiveCallTreeSourceGeneratorTests
{
	[Fact]
	public void VerifyGeneratedCodeCompiles()
	{
		var generator = new MassiveRecursiveCallTreeSourceGenerator();

		var inputCompilation = CreateEmptyCompilation();

		var driver = CSharpGeneratorDriver.Create(generator)
			.RunGeneratorsAndUpdateCompilation(
				inputCompilation,
				out var outputCompilation,
				out var diagnostics
				);
		Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

		var generatedTrees = outputCompilation.SyntaxTrees
			.Where(tree => !inputCompilation.SyntaxTrees.Contains(tree))
			.ToArray();

		Assert.NotEmpty(generatedTrees);

		var compilationDiagnostics = outputCompilation.GetDiagnostics();
		var errors = compilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

		if (errors.Any())
		{
			var errorMessages = string.Join("\n", errors.Select(e => $"{e.Location}: {e.GetMessage()}"));
			Assert.Fail($"Compilation errors found:\n{errorMessages}");
		}


		for (int i = 0; i < generatedTrees.Length; i++)
		{
			var expectedBuilder = new StringBuilder();
			expectedBuilder.AppendLine(
				$$"""
				  using System;
				  public static class Class{{i}}
				  {
				  	public static void Method0()
				  	{
				  		Console.WriteLine();
				  	}
				  """
				);
			for (int j = 1; j < generatedTrees.Length; j++)
			{
				expectedBuilder.AppendLine($"\tpublic static void Method{j}()");
				expectedBuilder.AppendLine("\t{");
				for (int k = 0; k < j; k++)
				{
					expectedBuilder.AppendLine($"\t\tMethod{k}();");
				}

				for (int k = 0; k < i; k++)
				{
					expectedBuilder.AppendLine($"\t\tClass{k}.Method{j}();");
				}
				expectedBuilder.AppendLine("\t}");
			}
			expectedBuilder.AppendLine("}");

			var tree = generatedTrees.Single(x => x.FilePath.EndsWith($"Class{i}.g.cs"));
			var source = tree.GetText().ToString();
			var expected = expectedBuilder.ToString();
			Assert.Equal(expected, source);
		}
	}

	public static CSharpCompilation CreateEmptyCompilation()
	{
		return CSharpCompilation.Create(
			assemblyName: "TestAssembly",
			syntaxTrees: new[] { CSharpSyntaxTree.ParseText("") }, // Empty source file
			references: new[]
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
				MetadataReference.CreateFromFile(
					typeof(object).Assembly.Location.Replace("System.Private.CoreLib.dll", "System.Runtime.dll")
					),
			},
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);
	}
}