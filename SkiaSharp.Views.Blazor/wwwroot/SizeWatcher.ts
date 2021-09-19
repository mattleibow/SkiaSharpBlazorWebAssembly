/// <reference path="types/dotnet/index.d.ts" />

type SizeWatcherElement = {
    SizeWatcher: SizeWatcherInstance;
} & HTMLElement

type SizeWatcherInstance = {
    callback: DotNet.DotNetObject;
}

export class SizeWatcher {
    static observer: ResizeObserver;

    public static observe(element: HTMLElement, callback: DotNet.DotNetObject) {
        if (!element || !callback)
            return;

        SizeWatcher.init();

        const watcherElement = element as SizeWatcherElement;
        watcherElement.SizeWatcher = {
            callback: callback
        };

        SizeWatcher.observer.observe(element);

        SizeWatcher.invokeAsync(element);
    }

    public static unobserve(element: HTMLElement) {
        if (!element || !SizeWatcher.observer)
            return;

        SizeWatcher.observer.unobserve(element);

        const watcherElement = element as SizeWatcherElement;
        watcherElement.SizeWatcher = undefined;
    }

    static init() {
        if (SizeWatcher.observer)
            return;

        SizeWatcher.observer = new ResizeObserver((entries) => {
            for (let entry of entries) {
                SizeWatcher.invokeAsync(entry.target);
            }
        });
    }

    static invokeAsync(element: Element): Promise<unknown> {
        const watcherElement = element as SizeWatcherElement;
        const instance = watcherElement.SizeWatcher;

        if (!instance || !instance.callback)
            return;

        return instance.callback.invokeMethodAsync('Invoke', element.clientWidth, element.clientHeight);
    }
}
