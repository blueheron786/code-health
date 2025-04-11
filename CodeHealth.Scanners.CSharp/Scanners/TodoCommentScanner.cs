using System.Text.Json;
using CodeHealth.Core.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeHealth.Scanners.CSharp.Scanners;

public class TodoCommentScanner : IStaticCodeScanner
{
    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            string filePath = kvp.Key;
            string code = kvp.Value;

            var tree = CSharpSyntaxTree.ParseText(code, path: filePath);
            var root = tree.GetRoot();

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/")
            };

            var todoComments = root
                .DescendantTrivia()
                .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia))
                .Where(t => t.ToString().Contains("// TODO") || t.ToString().Contains("//TODO"));

            foreach (var comment in todoComments)
            {
                var lineSpan = comment.GetLocation().GetLineSpan();
                var line = lineSpan.StartLinePosition.Line + 1;

                fileResult.Comments.Add(new CommentResult
                {
                    Line = line,
                    Text = comment.ToString().Trim()
                });

                report.TotalTodos++;
            }

            if (fileResult.Comments.Any())
                report.Files.Add(fileResult);
        }

        var outputFile = Path.Combine(outputDir, Constants.FileNames.TodoCommentsFile);
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(outputFile, JsonSerializer.Serialize(report, options));
    }

    private class Report
    {
        public int TotalTodos { get; set; }
        public List<FileResult> Files { get; set; } = new();
    }

    private class FileResult
    {
        public string File { get; set; } = string.Empty;
        public List<CommentResult> Comments { get; set; } = new();
    }

    private class CommentResult
    {
        public int Line { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
