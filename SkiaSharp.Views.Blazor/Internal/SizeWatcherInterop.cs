using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SizeWatcherInterop : IAsyncDisposable
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SizeWatcher.js";
		private const string ObserveSymbol = "SizeWatcher.observe";
		private const string UnobserveSymbol = "SizeWatcher.unobserve";

		private readonly Lazy<Task<IJSObjectReference>> moduleTask;

		private ElementReference htmlElement;
		private DotNetObjectReference<SizeActionHelper>? reference;

		public SizeWatcherInterop(IJSRuntime js)
		{
			if (moduleTask != null)
				return;

			moduleTask = new(() => js.InvokeAsync<IJSObjectReference>("import", JsFilename).AsTask());
		}

		public async ValueTask DisposeAsync()
		{
			if (!moduleTask.IsValueCreated)
				return;

			await UnobserveAsync();

			var module = await moduleTask.Value;

			await module.DisposeAsync();
		}

		public async Task ObserveAsync(ElementReference element, Action<SKSize> callback)
		{
			var module = await moduleTask.Value;

			htmlElement = element;

			var helper = new SizeActionHelper(callback);
			reference = DotNetObjectReference.Create(helper);

			await module.InvokeVoidAsync(ObserveSymbol, element, reference);
		}

		public async Task UnobserveAsync()
		{
			if (!moduleTask.IsValueCreated || reference == null)
				return;

			var module = await moduleTask.Value;

			await module.InvokeVoidAsync(UnobserveSymbol, htmlElement);

			reference?.Dispose();
			reference = null;

			htmlElement = default;
		}
	}
}
