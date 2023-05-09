using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaSharpAccessViolation;

public class MyCanvas : SKCanvasView
{
    public Command DrawCommand;
    private SKBitmap _buffer;

    public SKBitmap Bitmap { get;private set; }

    public MyCanvas()
    {
        _buffer = new SKBitmap(256, 256);
        DrawCommand = new Command(DrawBitmap);
    }

    public void SetBitmap(SKBitmap bitmap)
    {
        Bitmap = bitmap;
        DrawBitmap();
        InvalidateSurface();
    }

    private void DrawBitmap()
    {
        _buffer = new SKBitmap(256, 256);
        var canvas = new SKCanvas(_buffer);
        _buffer.Erase(SKColors.White);

        using (SKPaint paint = new SKPaint())
        {
            canvas.DrawImage(SKImage.FromBitmap(Bitmap),new SKPoint(0,0), paint);
        }


    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.Clear(SKColors.White);
        e.Surface.Canvas.DrawBitmap(_buffer, 0, 0);

        base.OnPaintSurface(e);
    }
}
