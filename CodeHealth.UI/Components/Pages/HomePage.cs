using Microsoft.AspNetCore.Components;
using Microsoft.Win32; // For OpenFolderDialog
using System.Diagnostics;

namespace CodeHealth.UI;

public partial class HomePage : ComponentBase
{
    internal string folderPath = "No folder selected";

    internal async Task OpenFolderPicker()
    {
        // Use WPF's OpenFolderDialog
        var dialog = new OpenFolderDialog
        {
            Title = "Select a folder",
            Multiselect = false,
            InitialDirectory = "C:\\" // Optional: Default starting folder
        };

        // Show the dialog (synchronous in WPF)
        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            folderPath = dialog.FolderName;
            Debug.WriteLine($"Selected folder: {folderPath}");
            StateHasChanged(); // Update UI
        }
    }
}