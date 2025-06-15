using System;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Blazor.Diagrams.Components;

public partial class DiagramCanvas : IAsyncDisposable
{
    private DotNetObjectReference<DiagramCanvas>? _reference;
    private bool _shouldRender;

    protected ElementReference elementReference;

    [CascadingParameter] public BlazorDiagram? BlazorDiagram { get; set; } = null;
    private BlazorDiagram? BlazorDiagram_ActiveVersion;
    [Parameter] public RenderFragment? Widgets { get; set; }

    [Parameter] public RenderFragment? AdditionalSvg { get; set; }

    [Parameter] public RenderFragment? AdditionalHtml { get; set; }

    [Parameter] public string? Class { get; set; }

    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    public async ValueTask DisposeAsync()
    {
        UnSubscribe();

        if (_reference == null)
            return;

        try
        {

            if (elementReference.Id != null
                    && BlazorDiagram?.Container != null)
                await JSRuntime.UnobserveResizes(elementReference);
        }
        catch (JSDisconnectedException ex)
        {
            _ = ex;
            // Circuit is already disconnected,
        }
        _reference.Dispose();
    }

    private string GetLayerStyle(int order)
    {
        if (BlazorDiagram is null) return "";

        return FormattableString.Invariant(
            $"transform: translate({BlazorDiagram.Pan.X}px, {BlazorDiagram.Pan.Y}px) scale({BlazorDiagram.Zoom}); z-index: {order};");
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _reference = DotNetObjectReference.Create(this);        
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (BlazorDiagram_ActiveVersion != BlazorDiagram)
        {

            UnSubscribe();
            // Parameter has changed;
            BlazorDiagram_ActiveVersion = BlazorDiagram;
            Subscribe();

            if (this.HasFirstRendered)
            {
                BlazorDiagram_ActiveVersion?.SetContainer(await JSRuntime.GetBoundingClientRect(elementReference));            
            }
            
        }
    }
    void UnSubscribe()
    {
        if (BlazorDiagram_ActiveVersion is not null)
            BlazorDiagram_ActiveVersion.Changed -= OnDiagramChanged;
    }
    void Subscribe()
    {
        if (BlazorDiagram_ActiveVersion is not null)
            BlazorDiagram_ActiveVersion.Changed += OnDiagramChanged;
    }
    private bool HasFirstRendered;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            this.HasFirstRendered = true;
            BlazorDiagram_ActiveVersion?.SetContainer(await JSRuntime.GetBoundingClientRect(elementReference));
            await JSRuntime.ObserveResizes(elementReference, _reference!);
        }
    }

    [JSInvokable]
    public void OnResize(Rectangle rect)
    {
        BlazorDiagram_ActiveVersion?.SetContainer(rect);
    }

    protected override bool ShouldRender()
    {
        if (!_shouldRender) return false;

        _shouldRender = false;
        return true;
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        BlazorDiagram_ActiveVersion?.TriggerPointerDown(null, e.ToCore());
    }

    private void OnPointerMove(PointerEventArgs e)
    {
        BlazorDiagram_ActiveVersion?.TriggerPointerMove(null, e.ToCore());
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        BlazorDiagram_ActiveVersion?.TriggerPointerUp(null, e.ToCore());
    }

    private void OnKeyDown(KeyboardEventArgs e)
    {
        BlazorDiagram_ActiveVersion?.TriggerKeyDown(e.ToCore());
    }

    private void OnWheel(WheelEventArgs e)
    {
        BlazorDiagram_ActiveVersion?.TriggerWheel(e.ToCore());
    }

    private void OnDiagramChanged()
    {
        _shouldRender = true;
        InvokeAsync(StateHasChanged);
    }
}