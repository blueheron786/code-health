using CodeHealth.Scanners.CSharp.Formatters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class CyclomaticComplexityScanner
{
    public static void AnalyzeFiles(List<string> files, string rootPath, string outputDir)
    {
        var report = new CyclomaticComplexityJsonFormatter.Report();

        foreach (var file in files)
        {
            var code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var fileResult = new CyclomaticComplexityJsonFormatter.FileResult
            {
                File = Path.GetRelativePath(rootPath, file).Replace("\\", "/")
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

                fileResult.Methods.Add(new CyclomaticComplexityJsonFormatter.MethodResult
                {
                    Method = method.Identifier.Text,
                    Complexity = complexity
                });

                report.TotalComplexity += complexity;
            }

            if (fileResult.Methods.Any())
                report.Files.Add(fileResult);
        }

        int methodCount = report.Files.Sum(f => f.Methods.Count);
        report.AverageComplexity = methodCount > 0 ? (double)report.TotalComplexity / methodCount : 0;

        var outputFile = Path.Combine(outputDir, "cyclomatic_complexity.json");
        CyclomaticComplexityJsonFormatter.WriteReport(outputFile, report);
    }
}
