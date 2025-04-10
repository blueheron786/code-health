using System.Text.Json;

namespace CodeHealth.Scanners.CSharp.Formatters;

public class CyclomaticComplexityJsonFormatter
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

    public static void WriteReport(string outputPath, Report report)
    {
        using var fs = File.Create(outputPath);
        using var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();

        writer.WritePropertyName("Files");
        writer.WriteStartArray();
        foreach (var file in report.Files)
        {
            writer.WriteStartObject();
            writer.WriteString("File", file.File);

            writer.WritePropertyName("Methods");
            writer.WriteStartArray();
            foreach (var method in file.Methods)
            {
                writer.WriteStartObject();
                writer.WriteString("Method", method.Method);
                writer.WriteNumber("Complexity", method.Complexity);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteNumber("TotalComplexity", report.TotalComplexity);
        writer.WriteNumber("AverageComplexity", report.AverageComplexity);
        writer.WriteEndObject();
    }
}
