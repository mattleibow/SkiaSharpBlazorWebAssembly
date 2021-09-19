/// <reference path="types/dotnet/index.d.ts" />
export class SizeWatcher {
    static observe(element, callback) {
        if (!element || !callback)
            return;
        SizeWatcher.init();
        const watcherElement = element;
        watcherElement.SizeWatcher = {
            callback: callback
        };
        SizeWatcher.observer.observe(element);
        SizeWatcher.invokeAsync(element);
    }
    static unobserve(element) {
        if (!element || !SizeWatcher.observer)
            return;
        SizeWatcher.observer.unobserve(element);
        const watcherElement = element;
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
    static invokeAsync(element) {
        const watcherElement = element;
        const instance = watcherElement.SizeWatcher;
        if (!instance || !instance.callback)
            return;
        return instance.callback.invokeMethodAsync('Invoke', element.clientWidth, element.clientHeight);
    }
}
