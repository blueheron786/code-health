using System.Text.Json;
using System.Text.RegularExpressions;
using CodeHealth.Core.Dtos.TodoComments;
using CodeHealth.Core.IO;

namespace CodeHealth.Scanners.Common
{
    public class TodoCommentScanner : IStaticCodeScanner
    {
        private static readonly Regex TodoRegex = new Regex(@"//\s*TODO", RegexOptions.Compiled);

        public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
        {
            var report = new TodoCommentsReport();

            foreach (var kvp in sourceFiles)
            {
                string filePath = kvp.Key;
                string code = kvp.Value;

                var fileResult = new FileResult
                {
                    File = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/")
                };

                // Find all TODO comments using regex
                var matches = TodoRegex.Matches(code);
                foreach (Match match in matches)
                {
                    var line = code.Substring(0, match.Index).Split('\n').Length; // Calculate the line number

                    fileResult.Comments.Add(new CommentResult
                    {
                        Line = line,
                    });

                    report.TotalTodos++;
                }

                if (fileResult.Comments.Any())
                {
                    report.Files.Add(fileResult);
                }
            }

            // Finalize and output the report
            var outputFile = Path.Combine(outputDir, Constants.FileNames.TodoCommentsFile);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(outputFile, JsonSerializer.Serialize(report, options));
        }
    }
}
