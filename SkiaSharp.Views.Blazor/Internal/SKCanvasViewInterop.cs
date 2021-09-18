using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SKCanvasViewInterop : IAsyncDisposable
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKCanvasViewInterop.js";
		private const string InvalidateSymbol = "SKCanvasView.invalidateCanvas";

		private readonly Lazy<Task<IJSObjectReference>> moduleTask;

		public SKCanvasViewInterop(IJSRuntime js)
		{
			moduleTask = new(() => js.InvokeAsync<IJSObjectReference>("import", JsFilename).AsTask());
		}

		public async ValueTask DisposeAsync()
		{
			if (!moduleTask.IsValueCreated)
				return;

			var module = await moduleTask.Value;

			await module.DisposeAsync();
		}

		public async Task<bool> InvalidateCanvasAsync(ElementReference htmlCanvas, IntPtr intPtr, SKSizeI canvasSize, SKSizeI rawSize)
		{
			var module = await moduleTask.Value;

			return await module.InvokeAsync<bool>(
				InvalidateSymbol,
				htmlCanvas,
				intPtr.ToInt64(),
				canvasSize.Width, canvasSize.Height,
				rawSize.Width, rawSize.Height);
		}
	}
}
