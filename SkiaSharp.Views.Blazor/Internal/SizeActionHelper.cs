using Microsoft.JSInterop;
using System;
using System.ComponentModel;

namespace SkiaSharp.Views.Blazor.Internal
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SizeActionHelper
	{
		private Action<SKSize> action;

		public SizeActionHelper(Action<SKSize> action)
		{
			this.action = action;
		}

		[JSInvokable]
		public void Invoke(float width, float height) => action?.Invoke(new SKSize(width, height));
	}
}