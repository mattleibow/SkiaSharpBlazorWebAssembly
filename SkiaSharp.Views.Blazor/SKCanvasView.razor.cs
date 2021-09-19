using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor
{
	public partial class SKCanvasView : IAsyncDisposable
	{
		private SKCanvasViewInterop interop = null!;
		private SizeWatcherInterop sizeWatcher = null!;
		private ElementReference htmlCanvas;

		private SKSizeI pixelSize;
		private byte[]? pixels;
		private GCHandle pixelsHandle;
		private bool ignorePixelScaling;
		private double dpi;
		private SKSize canvasSize;

		[Inject]
		IJSRuntime JS { get; set; } = null!;

		[Parameter]
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling != value)
				{
					ignorePixelScaling = value;
					Invalidate();
				}
			}
		}

		[Parameter]
		public EventCallback<SKPaintSurfaceEventArgs> OnPaintSurface { get; set; }

		[Parameter(CaptureUnmatchedValues = true)]
		public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				interop = new SKCanvasViewInterop(JS);

				DpiWatcherInterop.Init(JS);
				DpiWatcherInterop.DpiChanged += OnDpiChanged;

				sizeWatcher = new SizeWatcherInterop(JS);
				await sizeWatcher.ObserveAsync(htmlCanvas, OnSizeChanged);

				OnDpiChanged(await DpiWatcherInterop.GetDpiAsync());
			}
		}

		protected virtual Task InvokeOnPaintSurfaceAsync(SKPaintSurfaceEventArgs e)
		{
			return OnPaintSurface.InvokeAsync(e);
		}

		public async void Invalidate()
		{
			await InvalidateAsync();
		}

		public async Task InvalidateAsync()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
				return;

			var info = CreateBitmap(out var unscaledSize);
			var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;

			using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				if (IgnorePixelScaling)
				{
					var canvas = surface.Canvas;
					canvas.Scale((float)dpi);
					canvas.Save();
				}

				await InvokeOnPaintSurfaceAsync(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
			}

			await interop.InvalidateCanvasAsync(htmlCanvas, pixelsHandle.AddrOfPinnedObject(), info.Size);
		}

		private SKImageInfo CreateBitmap(out SKSizeI unscaledSize)
		{
			var size = CreateSize(out unscaledSize);
			var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);

			if (pixels == null || pixelSize.Width != info.Width || pixelSize.Height != info.Height)
			{
				FreeBitmap();

				pixels = new byte[info.BytesSize];
				pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
				pixelSize = info.Size;
			}

			return info;
		}

		private SKSizeI CreateSize(out SKSizeI unscaledSize)
		{
			unscaledSize = SKSizeI.Empty;

			var w = canvasSize.Width;
			var h = canvasSize.Height;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			unscaledSize = new SKSizeI((int)w, (int)h);
			return new SKSizeI((int)(w * dpi), (int)(h * dpi));

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}

		private void FreeBitmap()
		{
			if (pixels != null)
			{
				pixelsHandle.Free();
				pixels = null;
			}
		}

		private void OnDpiChanged(double newDpi)
		{
			dpi = newDpi;

			Invalidate();
		}

		private void OnSizeChanged(SKSize newSize)
		{
			canvasSize = newSize;

			Invalidate();
		}

		public async ValueTask DisposeAsync()
		{
			DpiWatcherInterop.DpiChanged -= OnDpiChanged;

			await sizeWatcher.DisposeAsync();
		}
	}
}