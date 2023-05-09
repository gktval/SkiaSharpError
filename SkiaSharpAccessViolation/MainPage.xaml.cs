using CommunityToolkit.Maui.Views;
using ProjNet.CoordinateSystems;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace SkiaSharpAccessViolation;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {

        var bitmapCopy = await CreateBitmap();

        canvas.SetBitmap(bitmapCopy);

    }

    private async Task<CoordinateSystem> PromptForCoordinateSystem()
    {
        var popup = new PopupCoordSys();
        popup.CanBeDismissedByTappingOutsideOfPopup = false;
        CoordinateSystem cs = (CoordinateSystem)await App.Current.MainPage.ShowPopupAsync(popup);
        return cs;
    }

    public async Task<SKBitmap> CreateBitmap()
    {
        int width = 805;
        int height = 616;

        float[,] singleArray = new float[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                singleArray[x, y] = x * y;
            }
        }

        var colorTable = new System.Drawing.Color[256];
        for (int i = 0; i < 256; i++)
            colorTable[i] = System.Drawing.Color.FromArgb(i, i, i);

        SKBitmap bitmap = ImageMethods.Instance.Create8BitSKBitmapFromSingleArray(singleArray, 0, width * height, colorTable);

        //This works
        //await DisplayAlert("Something", "Ask Something", "Yes");

        //This does not
        var cs = await PromptForCoordinateSystem();

        return bitmap.Copy();
    }

    
}

