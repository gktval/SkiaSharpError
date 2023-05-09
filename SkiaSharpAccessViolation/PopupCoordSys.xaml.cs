
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using ProjNet.CoordinateSystems;
using ProjNet.IO;
using ProjNet.IO.CoordinateSystems;
using ProjNet.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SkiaSharpAccessViolation;

public partial class PopupCoordSys : Popup
{
    public ICommand OKCommand { get; set; }
    public ICommand CancelCommand { get; set; }
    public ICommand SearchEPSGCommand { get; set; }
    public ICommand PerformSearch { get; set; }
    public ICommand NoResultsCommand { get; set; }

    public int SearchViewHeight { get; set; }
    public int SearchViewWidth { get; set; }
    public bool SearchVisible { get; set; }
    public string EPSG { get; set; }
    public string Name { get; set; }
    public string Alias { get; set; }
    public string Selection { get; set; }
    public bool HistoryVisible { get; set; }
    public CoordinateSystem CoordinateSystem { get; private set; }

    public List<CoordinateSystemInfo> SearchResults { get; set; }
    public ObservableCollection<string> PreviousCoordinateSystems { get; set; }

    private List<string> _prevCoordSystem;
    protected readonly DatabaseCoordinateService _service;

    public PopupCoordSys()
    {
        InitializeComponent();
        int width = 400;
        int height = 600;
#if WINDOWS
        var platformView = (Microsoft.Maui.Controls.Platform.ShellView)Application.Current.MainPage.Handler.PlatformView;
        width = (int)Math.Min(width, platformView.ActualSize.X)* 7/8;
        height = (int)(Math.Min(height, platformView.ActualSize.Y) * 7/8);
#else
        var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
        width = (int)Math.Min(width, mainDisplayInfo.Width / mainDisplayInfo.Density) * 7 / 8;
        height = (int)(Math.Min(height, mainDisplayInfo.Height / mainDisplayInfo.Density) * 7 / 8);
#endif

        Size = new Size(width, height);

        _service = new DatabaseCoordinateService();

        _prevCoordSystem = new List<string> { "4326 - WGS 84" };
        HistoryResultView.ItemsSource = _prevCoordSystem;
        PreviousCoordinateSystems = new ObservableCollection<string>();
        foreach (var system in _prevCoordSystem)
            PreviousCoordinateSystems.Add(system);
        HistoryVisible = PreviousCoordinateSystems.Count > 0;

        if (HistoryVisible)
            Size = new Size(width, height + 50);

        OKCommand = new Command(OnOKClick);
        CancelCommand = new Command(OnCancelClick);
        SearchEPSGCommand = new Command(SearchEPSG);
        PerformSearch = new Command(SearchName);
        NoResultsCommand = new Command(NoResults);

        SearchResults = new List<CoordinateSystemInfo>();
        BindingContext = this;

        this.Opened += OnOpened;
    }

    private void OnOpened(object sender, PopupOpenedEventArgs e)
    {
        SearchViewHeight = (int)Size.Height - 120;
        SearchViewWidth = (int)Size.Width - 40;
        OnPropertyChanged(nameof(SearchViewHeight));
        OnPropertyChanged(nameof(SearchViewWidth));
    }

    private void OnOKClick(object obj)
    {
        var prevCoord = _prevCoordSystem.Where(f => f == Selection).FirstOrDefault();
        if (prevCoord == null && !string.IsNullOrEmpty(Selection))
        {
            _prevCoordSystem.Insert(0, Selection);
            if (_prevCoordSystem.Count > 5)
                _prevCoordSystem.RemoveAt(4);
            //Globals.Settings.Save();
        }
        Close(CoordinateSystem);
    }

    private void OnCancelClick(object obj)
    {
        Close(null);
    }

    private async void SearchEPSG(object obj)
    {
        if (int.TryParse(EPSG, out int value))
        {
            CoordinateSystem = await GetCoordinateSystemAsync(value);
            Alias = CoordinateSystem.Alias;
        }
        else
        {
            errorMessage.Text = "Invalid EPSG code. The code must be numeric.";
            errorMessage.TextColor = Colors.Red;
        }
    }

    private async void SearchName(object obj)
    {
        SearchResults.Clear();
        if (int.TryParse(searchBar.Text, out var srid))
        {
            if (searchBar.Text.Length == 3) //there will no no codes with 3 digits
                return;
            var result = await GetCoordinateSystemAsync(srid);
            if (result != null)
            {
                SearchResults.Add(new CoordinateSystemInfo(result.Name, result.Alias, result.Authority, (int)result.AuthorityCode, result.Remarks, false, result.WKT));
            }
        }
        else
        {
            var result = await SearchCoordinateSystemAsync(searchBar.Text);
            SearchResults = result.ToList();
        }

        // More MAUI bugs where lists dont update in the collection view
        var itemSouce = new List<string>();
        foreach (var item in SearchResults)
        {
            itemSouce.Add(string.Format("{0} - {1}", item.Code.ToString(), item.Name));
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ObjectResultView.ItemsSource = itemSouce;
        });
    }

    private void NoResults(object obj)
    {
        System.Diagnostics.Debug.WriteLine("No Results: " + searchBar.Text);
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Text Changed: " + searchBar.Text);
        if (searchBar.Text == string.Empty && SearchResults.Count > 0)
        {
            var itemSouce = new List<string>();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ObjectResultView.ItemsSource = itemSouce;
                ObjectResultView.SelectedItem = null;

                if (SearchVisible)
                {
                    SearchVisible = false;
                    OnPropertyChanged(nameof(SearchVisible));
                }

            });
        }
        else
        {
            if (!SearchVisible)
            {
                SearchVisible = true;
                OnPropertyChanged(nameof(SearchVisible));
            }
        }
    }

    private void ObjectResultView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        Selection = e.CurrentSelection[0].ToString();
        string code = e.CurrentSelection[0].ToString().Split('-')[0].Trim();
        var info = SearchResults.Where(f => f.Code.ToString() == code).FirstOrDefault();
        CoordinateSystem = GetCoordinateSystem(info.WKT);

        var itemSouce = new List<string>();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Name = CoordinateSystem.Name;
            Alias = CoordinateSystem.Alias;
            EPSG = CoordinateSystem.AuthorityCode.ToString();
            entryCode.Text = EPSG;
            entryName.Text = Name;
            ObjectResultView.ItemsSource = itemSouce;
            ObjectResultView.SelectedItem = null;
        });

        if (SearchVisible)
        {
            SearchVisible = false;
            OnPropertyChanged(nameof(SearchVisible));
        }

   }

    private async void HistoryResultView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        try
        {
            string code = e.SelectedItem.ToString().Split('-')[0].Trim();
            if (int.TryParse(code, out int srid))
            {
                CoordinateSystem = await GetCoordinateSystemAsync(srid);

                var itemSouce = new List<string>();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Name = CoordinateSystem.Name;
                    Alias = CoordinateSystem.Alias;
                    EPSG = CoordinateSystem.AuthorityCode.ToString();
                    entryCode.Text = EPSG;
                    entryName.Text = Name;
                    ObjectResultView.ItemsSource = itemSouce;
                    ObjectResultView.SelectedItem = null;
                });
            }
        }
        catch
        {

        }
    }


    public async Task<CoordinateSystem> GetCoordinateSystemAsync(int srid)
    {
        if (srid == 0)
            return null;

        return await _service.GetCoordinateSystemAsync(srid);
    }

    public async Task<IEnumerable<CoordinateSystemInfo>> SearchCoordinateSystemAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return new CoordinateSystemInfo[] { };

        return await _service.SearchCoordinateSystemAsync(name);
    }

    public static CoordinateSystem GetCoordinateSystem(string wkt)
    {
        return CoordinateSystemWktReader.Parse(wkt) as CoordinateSystem;
    }
}
