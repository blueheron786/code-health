using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

public class CyclomaticComplexityScanner
{
    public class MethodResult
    {
        public string Method { get; set; } = "";
        public int Complexity { get; set; }
    }

    public class FileResult
    {
        public string File { get; set; } = "";
        public List<MethodResult> Methods { get; set; } = new();
    }

    public class Report
    {
        public List<FileResult> Files { get; set; } = new();
        public int TotalComplexity { get; set; }
        public double AverageComplexity { get; set; }
    }

    public static void AnalyzeFiles(List<string> files, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var file in files)
        {
            var code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, file).Replace("\\", "/")
            };

            var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methodNodes)
            {
                var complexity = 1; // base complexity
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
                        || n is BinaryExpressionSyntax bin && (bin.IsKind(SyntaxKind.LogicalAndExpression) || bin.IsKind(SyntaxKind.LogicalOrExpression))
                        || n is CatchClauseSyntax
                    );
                }

                var methodName = method.Identifier.Text;
                fileResult.Methods.Add(new MethodResult
                {
                    Method = methodName,
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
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputFile, json);
    }
}
