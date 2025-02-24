using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PowerOfTenAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoopBoundsAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "PT0201";

	private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
        "Loop bounds should be trivially identifiable",
		"Loop has sophisticated iteration logic, and such logic can cause loops to spin indefinitely. Avoid modifying the loop iterator, using a complex iteration expression, or breaking out of an intentionally infinite loop.",
		Category,
		DiagnosticSeverity.Warning, isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
		ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSemanticModelAction(AnalyzeSemanticModel);
	}

	private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
	{
		var semanticModel = context.SemanticModel;
		var root = semanticModel.SyntaxTree.GetRoot();
		var nodes = root.DescendantNodes();
		foreach (var node in nodes.OfType<WhileStatementSyntax>())
			AnalyzeWhileStatement(context, node);
	}

	private void AnalyzeWhileStatement(SemanticModelAnalysisContext context, WhileStatementSyntax node)
	{
		var childNodes = node.ChildNodes().ToList();
		if (childNodes.Count != 2)
		{
			var diagnostic = Diagnostic.Create(Rule, node.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}

		if (node.Condition is LiteralExpressionSyntax trueLiteral &&
		    trueLiteral.IsKind(SyntaxKind.TrueLiteralExpression))
		{
			// Infinite loop--look for breaks and returns
			var breakNodes = node.Statement.DescendantNodes().Prepend(node.Statement).OfType<BreakStatementSyntax>().ToList();
			var breakApplications = new SyntaxKind[]
			{
				SyntaxKind.ForStatement,
				SyntaxKind.ForEachStatement,
				SyntaxKind.WhileStatement,
				SyntaxKind.DoStatement,
				SyntaxKind.SwitchStatement
			};
			
			foreach (var breakNode in breakNodes)
			{
				var breakLoopParent = breakNode.Parent;
				while (breakLoopParent != null && breakLoopParent != node)
				{
					var kind = breakLoopParent.Kind();
					if (breakApplications.Contains(kind))
						break;
					breakLoopParent = breakLoopParent.Parent;
				}

				if (breakLoopParent == node)
				{
					context.ReportDiagnostic(Diagnostic.Create(Rule, breakNode.GetLocation()));
				}
			}
			
			var returnNodes = node.Statement.DescendantNodes().Prepend(node.Statement).OfType<ReturnStatementSyntax>().ToList();
			foreach (var returnNode in returnNodes)
			{
				var diagnostic = Diagnostic.Create(Rule, returnNode.GetLocation());
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}