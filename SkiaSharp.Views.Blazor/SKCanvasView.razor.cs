using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor
{
	public partial class SKCanvasView
	{
		private SKCanvasViewInterop interop;
		private SKSizeI pixelSize;
		private byte[] pixels;
		private GCHandle pixelsHandle;
		private bool ignorePixelScaling;
		private ElementReference htmlCanvas;

		[Inject]
		IJSRuntime JS { get; set; }

		[Parameter]
		public EventCallback<SKPaintSurfaceEventArgs> OnPaintSurface { get; set; }

		[Parameter]
		public double ActualWidth { get; set; }

		[Parameter]
		public double ActualHeight { get; set; }

		double Dpi { get; } = 1;

		public SKSize CanvasSize { get; private set; }

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				//Invalidate();
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				interop = new SKCanvasViewInterop(JS, htmlCanvas);

				await InvalidateAsync();
			}
		}

		protected virtual Task InvokeOnPaintSurfaceAsync(SKPaintSurfaceEventArgs e)
		{
			return OnPaintSurface.InvokeAsync(e);
		}

		public async Task InvalidateAsync()
		{
			if (ActualWidth <= 0 || ActualHeight <= 0)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			var info = CreateBitmap(out var unscaledSize, out var dpi);

			using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;

				CanvasSize = userVisibleSize;

				if (IgnorePixelScaling)
				{
					var canvas = surface.Canvas;
					canvas.Scale(dpi);
					canvas.Save();
				}

				await InvokeOnPaintSurfaceAsync(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
			}

			await interop.InvalidateCanvasAsync(pixelsHandle.AddrOfPinnedObject(), info);
		}

		private SKImageInfo CreateBitmap(out SKSizeI unscaledSize, out float dpi)
		{
			var size = CreateSize(out unscaledSize, out dpi);
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

		private SKSizeI CreateSize(out SKSizeI unscaledSize, out float dpi)
		{
			unscaledSize = SKSizeI.Empty;
			dpi = (float)Dpi;

			var w = ActualWidth;
			var h = ActualHeight;

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
	}
}