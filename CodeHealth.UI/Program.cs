using Microsoft.AspNetCore.Components.Web;
using BlazorDesktop.Hosting;
using CodeHealth.UI.Components;
using System.IO;
using CodeHealth.Core.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using CodeHealth.UI;

var builder = BlazorDesktopHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.UseDeveloperTools();
}

// TODO: move this elsewhere. For now, test against the CodeHealth code base.
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

var app = builder.Build();

await app.RunAsync();