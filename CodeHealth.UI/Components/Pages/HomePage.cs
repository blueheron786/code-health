using CodeHealth.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Win32; // For OpenFolderDialog
using System.Diagnostics;

namespace CodeHealth.UI;

public partial class HomePage : ComponentBase
{
    internal string outputMessages = "No folder selected";

    internal void OpenFolderPicker()
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
            var folderPath = dialog.FolderName;
            Debug.WriteLine($"Selected folder: {folderPath}");
            StateHasChanged(); // Update UI

            try
            {
                var results = ProjectScanner.Scan(folderPath);
                outputMessages = $"Scan of {folderPath} done in {results}";
            }
            catch (Exception ex)
            {
                outputMessages = $"Scan failed: {ex.Message}!";
            }
        }
        else {
            // User cancelled
        }
    }
}