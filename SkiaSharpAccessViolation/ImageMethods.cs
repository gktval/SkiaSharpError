using SkiaSharp;
using System.Runtime.InteropServices;

namespace SkiaSharpAccessViolation;

public  class ImageMethods
{
    public static ImageMethods Instance = new ImageMethods();

    internal static SKBitmap ApplyAffine(SKBitmap origBitmap)
    {
        var bitmap = origBitmap.Copy();

        // Affine transform
        SKMatrix affine = new SKMatrix
        {
            ScaleX = 1.1f,
            SkewY = 15,
            SkewX = -15,
            ScaleY = 1.15f,

            TransX = Math.Max(0, -8),
            TransY = Math.Max(0, -5),
            Persp2 = 1
        };

        var newBitmap = new SKBitmap(256, 256);

        using (var canvas = new SKCanvas(newBitmap))
        {
            using (var paint = new SKPaint())
            {
                paint.FilterQuality = SKFilterQuality.High;

                canvas.SetMatrix(affine);
                canvas.DrawBitmap(bitmap, 0, 0, paint);
                canvas.Restore();
            }
        }

        return newBitmap;
    }

    public SKBitmap Create8BitSKBitmapFromSingleArray(float[,] SingleArray, float MinVal, float MaxVal, System.Drawing.Color[] colorTable)
    {

        // Takes Input of a Single Array and a Desired Min and Max, if CalcMinMax is true the new Min Max is Returned in the
        // ByRef Min Max Values
        int width = SingleArray.GetUpperBound(0);
        int height = SingleArray.GetUpperBound(1);

        var info = new SKImageInfo(width + 1, height + 1, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
        var outbmp = new SKBitmap(info);

        double SngPixVal;
        double DoubleWriteVal;
        double Multiplier;
        byte WriteVal;
        int Readloc;

        var BMArray = new byte[(info.RowBytes * info.Height)];

        Multiplier = (double)((MaxVal - MinVal) / 254f);

        if (MaxVal == MinVal)
        {
            MaxVal = Convert.ToSingle((double)MaxVal + (double)MaxVal * 0.01d);
            Multiplier = (double)((MaxVal - MinVal) / 254f);
        }

        for (int Y = 0; Y <= height; Y++)
        {
            for (int X = 0; X <= width; X++)
            {

                SngPixVal = SingleArray[X, Y];

                if (SngPixVal == Single.MinValue)
                {
                    WriteVal = 255;
                    Readloc = Y * outbmp.RowBytes + X * outbmp.BytesPerPixel;
                    BMArray[Readloc] = WriteVal;
                    BMArray[Readloc + 1] = WriteVal;
                    BMArray[Readloc + 2] = WriteVal;
                    BMArray[Readloc + 3] = 255;
                }
                else if ((double)(MaxVal + MinVal) + Multiplier == 0d)
                {
                    SngPixVal = Single.MinValue;
                    SingleArray[X, Y] = Single.MinValue;
                    WriteVal = 255;
                    Readloc = Y * outbmp.RowBytes + X * outbmp.BytesPerPixel;
                    BMArray[Readloc] = colorTable[WriteVal].B;
                    BMArray[Readloc + 1] = colorTable[WriteVal].G;
                    BMArray[Readloc + 2] = colorTable[WriteVal].R;
                    BMArray[Readloc + 3] = colorTable[WriteVal].A;
                }
                else
                {
                    if (float.IsInfinity((float)SngPixVal) || float.IsNaN((float)SngPixVal))
                    {
                        SngPixVal = Single.MinValue;
                    }
                    if (SngPixVal > (double)MaxVal)
                        SngPixVal = (double)Convert.ToSingle(MaxVal);
                    if (SngPixVal < (double)MinVal)
                        SngPixVal = (double)Convert.ToSingle(MinVal);
                    DoubleWriteVal = (SngPixVal - (double)MinVal) / Multiplier;
                    WriteVal = Convert.ToByte(DoubleWriteVal);
                    Readloc = Y * outbmp.RowBytes + X * outbmp.BytesPerPixel;
                    BMArray[Readloc] = colorTable[WriteVal].B;
                    BMArray[Readloc + 1] = colorTable[WriteVal].G;
                    BMArray[Readloc + 2] = colorTable[WriteVal].R;
                    BMArray[Readloc + 3] = colorTable[WriteVal].A;
                }
            }
        }

        // pin the managed array so that the GC doesn't move it
        var handle = GCHandle.Alloc(BMArray, GCHandleType.Pinned);

        // install the pixels with the color type of the pixel data
        outbmp.InstallPixels(info, handle.AddrOfPinnedObject(), info.RowBytes);

        handle.Free();
        GC.Collect();

        return outbmp;

    }
}
