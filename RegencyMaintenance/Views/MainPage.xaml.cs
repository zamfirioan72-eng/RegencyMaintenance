using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RegencyMaintenance.Helpers;
using RegencyMaintenance.Services;

namespace RegencyMaintenance.Views;

public sealed partial class MainPage : Page
{
    private DispatcherTimer? _timer;

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string operatorName)
            OperatorText.Text = $"👤 Operatore: {operatorName}";
        else
            OperatorText.Text = $"👤 Operatore: {DataService.Instance.CurrentOperator}";
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        BuildRoomGrid();
        StartClock();
    }

    private void StartClock()
    {
        UpdateDateTime();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => UpdateDateTime();
        _timer.Start();
    }

    private void UpdateDateTime()
    {
        DateTimeText.Text = $"📅 {DateHelper.GetDayOfWeekItalian()}, {DateHelper.NowFormatted}";
    }

    private void BuildRoomGrid()
    {
        var panels = new[] { Floor1Panel, Floor2Panel, Floor3Panel, Floor4Panel, Floor5Panel, Floor6Panel };
        for (int floor = 1; floor <= 6; floor++)
        {
            var rooms = DataService.GetFloorRooms(floor);
            var panel = panels[floor - 1];
            panel.Items.Clear();
            foreach (var room in rooms)
            {
                var btn = CreateRoomButton(room);
                panel.Items.Add(btn);
            }
        }
    }

    private Button CreateRoomButton(string room)
    {
        var btn = new Button
        {
            Content = room,
            Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xA5, 0xD6, 0xA7)),
            Foreground = new SolidColorBrush(Colors.Black),
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(4),
            CornerRadius = new CornerRadius(6),
            MinWidth = 90
        };
        btn.Click += RoomButton_Click;
        return btn;
    }

    private async void RoomButton_Click(object sender, RoutedEventArgs e)
    {
        string roomName;
        if (sender is Button btn)
            roomName = btn.Content?.ToString() ?? string.Empty;
        else
            return;

        // Strip emoji if present
        roomName = roomName.Replace("🏢 ", string.Empty).Trim();

        var dialog = new RoomDialog(roomName);
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }

    private async void GenerateReport_Click(object sender, RoutedEventArgs e)
    {
        var reportService = new ReportService();
        var path = await reportService.GenerateDailyReportAsync();
        var content = await File.ReadAllTextAsync(path);

        var dialog = new ReportPreviewDialog(content, path);
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }

    private async void PeriodFilter_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new PeriodFilterDialog();
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }

    private void ChangeOperator_Click(object sender, RoutedEventArgs e)
    {
        _timer?.Stop();
        Frame.Navigate(typeof(LoginPage));
    }

    private async void AllInterventions_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AllInterventionsDialog();
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }

    private async void BackupManagement_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new BackupDialog();
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }
}
