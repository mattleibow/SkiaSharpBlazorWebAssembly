/// <reference path="types/dotnet/index.d.ts" />

export class DpiWatcher {
    static lastDpi: number;
    static timerId: number;

    public static getDpi() {
        return window.devicePixelRatio;
    }

    public static start() {
        DpiWatcher.lastDpi = window.devicePixelRatio;
        DpiWatcher.timerId = window.setInterval(DpiWatcher.update, 1000);
    }

    public static stop() {
        window.clearInterval(DpiWatcher.timerId);
    }

    static update() {
        const currentDpi = window.devicePixelRatio;
        const lastDpi = DpiWatcher.lastDpi;
        DpiWatcher.lastDpi = currentDpi;

        if (Math.abs(lastDpi - currentDpi) > 0.001) {
            DotNet.invokeMethodAsync('SkiaSharp.Views.Blazor', 'UpdateDpi', lastDpi, currentDpi);
        }
    }
}
