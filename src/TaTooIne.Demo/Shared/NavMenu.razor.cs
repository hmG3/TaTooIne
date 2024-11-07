using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace TaTooIne.Demo.Shared;

public partial class NavMenu
{
    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
        GC.SuppressFinalize(this);
    }

    private async void HandleLocationChanged(object? sender, LocationChangedEventArgs args) => await ToggleNavMenuAsync(false);

    private async Task ToggleNavMenuAsync(bool isOpen) => await JsRuntime.InvokeVoidAsync("App.collapseNavMenu", isOpen, "menu");
}
