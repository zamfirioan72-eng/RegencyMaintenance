using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegencyMaintenance.Helpers;
using RegencyMaintenance.Models;
using RegencyMaintenance.Services;

namespace RegencyMaintenance.Views;

public sealed partial class PeriodFilterDialog : ContentDialog
{
    public PeriodFilterDialog()
    {
        this.InitializeComponent();
        this.Loaded += PeriodFilterDialog_Loaded;
    }

    private void PeriodFilterDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Set defaults: last 30 days
        ToDatePicker.Date = DateTimeOffset.Now;
        FromDatePicker.Date = DateTimeOffset.Now.AddDays(-30);

        // Populate room filter combo
        RoomFilterCombo.Items.Add("Tutte");
        foreach (var room in DataService.GetAllRoomNames())
            RoomFilterCombo.Items.Add(room);

        // Add categories
        var categories = DataService.Instance.Interventions
            .Select(i => i.Category)
            .Distinct()
            .OrderBy(c => c);
        foreach (var cat in categories)
            if (!RoomFilterCombo.Items.Contains(cat))
                RoomFilterCombo.Items.Add(cat);

        RoomFilterCombo.SelectedIndex = 0;
        ApplyFilter();
    }

    private void Filter_Click(object sender, RoutedEventArgs e) => ApplyFilter();
    private void Today_Click(object sender, RoutedEventArgs e)
    {
        FromDatePicker.Date = DateTimeOffset.Now;
        ToDatePicker.Date = DateTimeOffset.Now;
        ApplyFilter();
    }
    private void Last7Days_Click(object sender, RoutedEventArgs e)
    {
        FromDatePicker.Date = DateTimeOffset.Now.AddDays(-7);
        ToDatePicker.Date = DateTimeOffset.Now;
        ApplyFilter();
    }
    private void Last30Days_Click(object sender, RoutedEventArgs e)
    {
        FromDatePicker.Date = DateTimeOffset.Now.AddDays(-30);
        ToDatePicker.Date = DateTimeOffset.Now;
        ApplyFilter();
    }
    private void ShowAll_Click(object sender, RoutedEventArgs e)
    {
        FromDatePicker.Date = DateTimeOffset.MinValue.AddYears(1999);
        ToDatePicker.Date = DateTimeOffset.Now;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var from = FromDatePicker.Date.Date;
        var to = ToDatePicker.Date.Date;
        var roomFilter = RoomFilterCombo.SelectedItem?.ToString() ?? "Tutte";

        var results = DataService.Instance.Interventions
            .Where(i =>
            {
                if (!DateHelper.TryParseIsoDate(i.Date, out var d)) return false;
                return d >= from && d <= to;
            })
            .Where(i => roomFilter == "Tutte" ||
                        i.Room == roomFilter || i.Category == roomFilter)
            .OrderByDescending(i => i.Timestamp)
            .ToList();

        ResultsList.ItemsSource = results;
        ResultSummary.Text = $"Trovati {results.Count} interventi dal {from:dd/MM/yyyy} al {to:dd/MM/yyyy}";
    }

    private async void ExportTxt_Click(object sender, RoutedEventArgs e)
    {
        var items = (ResultsList.ItemsSource as List<Intervention>) ?? new List<Intervention>();

        var lines = new List<string>
        {
            "=== INTERVENTI PER PERIODO - THE REGENCY MAINTENANCE ===",
            $"Esportato: {DateHelper.NowFormatted}",
            $"Totale: {items.Count}",
            string.Empty
        };
        foreach (var inv in items)
            lines.Add($"{inv.DisplayDate} {inv.DisplayTime} | {inv.Room} | {inv.Category} | {inv.Note} | {inv.Operator}");

        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"periodo_{DateHelper.TimestampFile}.txt");
        await File.WriteAllLinesAsync(path, lines);

        var dlg = new ContentDialog
        {
            Title = "Esportato",
            Content = $"File salvato in:\n{path}",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dlg.ShowAsync();
    }
}
