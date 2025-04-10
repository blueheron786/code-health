using CodeHealth.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CodeHealth.UI;

public class HomePage : ComponentBase
{
    internal string folderPath;

    [Inject]
    private IJSRuntime _jsRuntime { get; set;}

    internal async Task OpenFolderPicker()
    {
        // Trigger folder picker logic here
        folderPath = await FolderPicker.PickFolderAsync(_jsRuntime);
        Console.WriteLine($"Picked folder: {folderPath}");
    }
}