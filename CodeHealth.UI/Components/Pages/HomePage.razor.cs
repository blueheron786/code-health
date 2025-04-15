using CodeHealth.Core.IO;
using Microsoft.AspNetCore.Components;
using System.IO;

namespace CodeHealth.UI;

public partial class HomePage : ComponentBase
{
    [Inject]
    private NavigationManager _navigation { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var projectFilePath = Constants.FileNames.ProjectsMetadataFile;

        // Only redirect if the current URL is the homepage ("/") and the project.xml file exists
        if (File.Exists(projectFilePath))
        {
            _navigation.NavigateTo("/Projects");
        }
    }
}