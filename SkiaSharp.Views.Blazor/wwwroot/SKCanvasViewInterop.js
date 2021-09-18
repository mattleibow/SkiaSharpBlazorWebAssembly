
export class SKCanvasView {
    static getDensity() {
        if ('devicePixelRatio' in window && window.devicePixelRatio > 0)
            return window.devicePixelRatio;
        return 1;
    }

    static invalidateCanvas(htmlCanvas, pData, canvasWidth, canvasHeight, width, height) {
        if (!htmlCanvas || !pData || width <= 0 || width <= 0)
            return false;

        var ctx = htmlCanvas.getContext('2d');
        if (!ctx)
            return false;

        const newWidth = canvasWidth + "px";
        const newHeight = canvasHeight + 'px';

        if (htmlCanvas.style.width != newWidth)
            htmlCanvas.style.width = newWidth;
        if (htmlCanvas.style.height != newHeight)
            htmlCanvas.style.height = newHeight;

        var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
        var imageData = new ImageData(buffer, width, height);
        ctx.putImageData(imageData, 0, 0);

        return true;
    }
}
