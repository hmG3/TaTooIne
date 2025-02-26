using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TALib;

namespace TaTooIne.Demo.DemoIndicators;

public partial class IndicatorsIndex
{
    private bool _navMenuOpened;
    private List<IGrouping<string, Abstract.IndicatorFunction>> _groups = null!;

    private int ActiveGroupId { get; set; } = -1;

    [Parameter]
    public EventCallback<Abstract.IndicatorFunction> OnIndicatorSelect { get; set; }

    protected override void OnInitialized()
    {
        _groups = Abstract.All.Where(g => g.Group != "Math Transform").GroupBy(f => f.Group).ToList();
    }

    private string? NavMenuCssClass(int groupId) => ActiveGroupId == groupId ? "active" : null;

    private void ToggleActive(int groupId) => ActiveGroupId = ActiveGroupId == groupId ? -1 : groupId;

    private async Task ToggleNavMenuAsync()
    {
        _navMenuOpened = !_navMenuOpened;
        await JsRuntime.InvokeVoidAsync("App.collapseNavMenu", _navMenuOpened, "toc");
    }
}
