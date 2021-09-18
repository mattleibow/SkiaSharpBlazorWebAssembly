using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor
{
	internal class SKCanvasViewInterop : IAsyncDisposable
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKCanvasViewInterop.js";
		private const string InvalidateSymbol = "SKCanvasView.invalidateCanvas";

		private readonly Lazy<Task<IJSObjectReference>> moduleTask;
		private readonly ElementReference htmlCanvas;

		public SKCanvasViewInterop(IJSRuntime js, ElementReference htmlCanvas)
		{
			moduleTask = new(() => js.InvokeAsync<IJSObjectReference>("import", JsFilename).AsTask());
			this.htmlCanvas = htmlCanvas;
		}

		public async ValueTask DisposeAsync()
		{
			if (!moduleTask.IsValueCreated)
				return;

			var module = await moduleTask.Value;

			await module.DisposeAsync();
		}

		public async Task<bool> InvalidateCanvasAsync(IntPtr intPtr, SKImageInfo info)
		{
			var module = await moduleTask.Value;

			return await module.InvokeAsync<bool>(InvalidateSymbol, intPtr.ToInt64(), htmlCanvas, info.Width, info.Height);
		}
	}
}
