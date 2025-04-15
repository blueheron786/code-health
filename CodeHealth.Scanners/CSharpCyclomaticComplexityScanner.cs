namespace CodeHealth.Scanners;

using CodeHealth.Core.Dtos;
using CodeHealth.Scanners.Common;
using CodeHealth.Scanners.Common.Scanners;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class CSharpCyclomaticComplexityScanner : IStaticCodeScanner
{
    public const string FileExtension = ".cs";

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            string fileName = kvp.Key;
            var code = kvp.Value;

            if (!fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methodNodes)
            {
                var complexity = 1;
                var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;

                if (body != null)
                {
                    var nodes = body.DescendantNodes();
                    complexity += nodes.Count(n =>
                        n is IfStatementSyntax
                        || n is ForStatementSyntax
                        || n is ForEachStatementSyntax
                        || n is WhileStatementSyntax
                        || n is DoStatementSyntax
                        || n is CaseSwitchLabelSyntax
                        || n is ConditionalExpressionSyntax
                        || n is BinaryExpressionSyntax bin &&
                            (bin.IsKind(SyntaxKind.LogicalAndExpression) || bin.IsKind(SyntaxKind.LogicalOrExpression))
                        || n is CatchClauseSyntax
                    );
                }

                var lineSpan = method.GetLocation().GetLineSpan();
                var startLine = lineSpan.StartLinePosition.Line + 1; // +1 for 1-based indexing
                var endLine = lineSpan.EndLinePosition.Line + 1;

                var issue = new IssueResult
                {
                    File = Path.GetRelativePath(rootPath, fileName).Replace("\\", "/"),
                    Line = startLine,
                    EndLine = endLine,
                    Name = method.Identifier.Text,
                    Metric = new Metric
                    {
                        Name = "Cyclomatic Complexity",
                        Value = complexity,
                        Threshold = 10 // Example threshold for high complexity
                    },
                    Message = $"Method '{method.Identifier.Text}' has a cyclomatic complexity of {complexity}.",
                    Severity = complexity > 10 ? "High" : "Medium", // Simple severity based on complexity value
                    Suggestion = "Consider refactoring the method to reduce complexity.",
                    Tags = new List<string> { "complexity", "refactor" },
                    Fixable = true // This can be updated later if refactoring suggestions are automated
                };

                report.Issues.Add(issue);
                report.TotalMetricValue += complexity;
            }
        }

        // Calculate Average Complexity
        report.AverageMetricValue = report.TotalMetricValue / (double)report.Issues.Count;

        // Finalize the report output (e.g., saving it to a JSON file)
        CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.csharp.json");
    }
}
