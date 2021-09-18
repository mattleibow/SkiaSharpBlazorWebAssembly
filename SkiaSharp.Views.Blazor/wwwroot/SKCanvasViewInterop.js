
export class SKCanvasView {
    static invalidateCanvas(pData, htmlCanvas, width, height) {
        if (!htmlCanvas)
            return false;

        htmlCanvas.width = width;
        htmlCanvas.height = height;

        var ctx = htmlCanvas.getContext('2d');
        if (!ctx)
            return false;

        var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
        var imageData = new ImageData(buffer, width, height);
        ctx.putImageData(imageData, 0, 0);

        return true;
    }
}
