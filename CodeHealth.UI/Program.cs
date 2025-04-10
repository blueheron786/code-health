using Microsoft.AspNetCore.Components.Web;
using BlazorDesktop.Hosting;
using CodeHealth.UI.Components;
using System.IO;
using CodeHealth.Core.IO;
using System.Reflection;
using System.Diagnostics;

var builder = BlazorDesktopHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.UseDeveloperTools();
}

// TODO: move this elsewhere
void ScanYourself() {
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    var sourcePath = Path.GetFullPath(Path.Combine(currentPath, @"..\..\..\.."));
    var mySourceFiles = FileDiscoverer.GetSourceFiles(sourcePath!);
    var resultsDirectory = RunInfo.CreateRun(DateTime.Now);

    Console.WriteLine($"Analyzing {sourcePath} ...");

    CyclomaticComplexityScanner.AnalyzeFiles(mySourceFiles, sourcePath, resultsDirectory);

    Console.WriteLine($"Analysis complete in {stopwatch.Elapsed}!");
}

ScanYourself();
await builder.Build().RunAsync();