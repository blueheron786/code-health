using BlazorDesktop.Wpf;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Comopnents.Pages;

public class RoutesPage : ComponentBase
{
    [Inject]
    private BlazorDesktopWindow _window { get; set; }
    
    override protected async Task OnInitializedAsync()
    {
        _window.WindowState = System.Windows.WindowState.Maximized;
    }
}