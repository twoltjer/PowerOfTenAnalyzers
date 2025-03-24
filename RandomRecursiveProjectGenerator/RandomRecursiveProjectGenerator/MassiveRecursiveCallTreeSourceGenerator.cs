using System.Text;
using Microsoft.CodeAnalysis;

namespace RandomRecursiveProjectGenerator;

[Generator]
public class MassiveRecursiveCallTreeSourceGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
	}

	public void Execute(GeneratorExecutionContext context)
	{
		const int n = 9;
		for (int classIndex = 0; classIndex < n; classIndex++)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("using System;");
			stringBuilder.AppendLine($"public static class Class{classIndex}");
			stringBuilder.AppendLine("{");
			for (int methodIndex = 0; methodIndex < n; methodIndex++)
			{
				stringBuilder.AppendLine($"\tpublic static void Method{methodIndex}()");
				stringBuilder.AppendLine("\t{");
				if (methodIndex == 0)
					stringBuilder.AppendLine("\t\tConsole.WriteLine();");
				else
				{
					for (int i = 0; i < methodIndex; i++)
					{
						stringBuilder.AppendLine($"\t\tMethod{i}();");
					}

					for (int i = 0; i < classIndex; i++)
					{
						stringBuilder.AppendLine($"\t\tClass{i}.Method{methodIndex}();");
					}
				}
				stringBuilder.AppendLine("\t}");
			}
			stringBuilder.AppendLine("}");
			var source = stringBuilder.ToString();
			context.AddSource($"Class{classIndex}.g.cs", source);
		}
	}
}