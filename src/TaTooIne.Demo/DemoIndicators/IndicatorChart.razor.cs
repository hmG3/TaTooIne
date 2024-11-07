using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TALib;

namespace TaTooIne.Demo.DemoIndicators;

public partial class IndicatorChart
{
    private ElementReference _chartCanvas;

    [Parameter]
    public required Function Function { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("App.setupChart", _chartCanvas);
        }
    }
}
