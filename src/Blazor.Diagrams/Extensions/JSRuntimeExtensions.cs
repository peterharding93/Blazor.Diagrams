using System;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Geometry;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Diagrams.Extensions;

public static class JSRuntimeExtensions
{
    public static async Task<Rectangle> GetBoundingClientRect(this IJSRuntime jsRuntime, ElementReference element)
    {
        return await jsRuntime.InvokeAsync<Rectangle>("ZBlazorDiagrams.getBoundingClientRect", element);
    }

    public static async Task ObserveResizes<T>(this IJSRuntime jsRuntime, ElementReference element,
        DotNetObjectReference<T> reference) where T : class
    {
        if (!jsRuntime.IsAvailable())
            return;

        try
        {
            await jsRuntime.InvokeVoidAsync("ZBlazorDiagrams.observe", element, reference, element.Id);
        }
        catch (ObjectDisposedException)
        {
            // Ignore, DotNetObjectReference was likely disposed
        }
        catch (JSDisconnectedException)
        {
            // Ignore, Blazor Server circuit was likely disconnected
        }
    }

    public static async Task UnobserveResizes(this IJSRuntime jsRuntime, ElementReference element)
    {
        if (!jsRuntime.IsAvailable())
            return;

        try
        {
            await jsRuntime.InvokeVoidAsync("ZBlazorDiagrams.unobserve", element, element.Id);
        }
        catch (ObjectDisposedException)
        {
            // Ignore, JS runtime was likely disposed
        }
        catch (JSDisconnectedException)
        {
            // Ignore, Blazor Server circuit was likely disconnected
        }
    }

    private static bool IsAvailable(this IJSRuntime jsRuntime)
    {
        var isInitializedProperty = jsRuntime.GetType().GetProperty("IsInitialized");
        return isInitializedProperty?.GetValue(jsRuntime) as bool? != false;
    }
}