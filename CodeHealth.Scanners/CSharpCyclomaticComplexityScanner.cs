namespace CodeHealth.Scanners;

using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Scanners.Common;
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
            var text = tree.GetText();

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, fileName).Replace("\\", "/")
            };

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

                fileResult.Methods.Add(new MethodResult
                {
                    Method = method.Identifier.Text,
                    Complexity = complexity,
                    StartLine = startLine,
                    EndLine = endLine
                });

                report.TotalComplexity += complexity;
            }

            if (fileResult.Methods.Any())
                report.Files.Add(fileResult);
        }

        CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.csharp.json");
    }
}
