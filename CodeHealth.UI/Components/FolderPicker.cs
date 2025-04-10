namespace CodeHealth.UI.Components;

using Microsoft.JSInterop;
using System.Threading.Tasks;

public static class FolderPicker
{
    public static async Task<string> PickFolderAsync(IJSRuntime jsRuntime)
    {
        var folderPath = await jsRuntime.InvokeAsync<string>("blazorFolderPicker.pickFolder");
        return folderPath;
    }
}
