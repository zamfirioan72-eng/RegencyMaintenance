using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegencyMaintenance.Helpers;
using RegencyMaintenance.Models;
using RegencyMaintenance.Services;

namespace RegencyMaintenance.Views;

public sealed partial class AllInterventionsDialog : ContentDialog
{
    public AllInterventionsDialog()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) => LoadInterventions();
    }

    private void LoadInterventions()
    {
        var interventions = DataService.Instance.Interventions
            .OrderByDescending(i => i.Timestamp)
            .ToList();

        InterventionsList.ItemsSource = interventions;
        SummaryText.Text = $"Totale interventi: {interventions.Count}";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Intervention intervention)
        {
            var confirm = new ContentDialog
            {
                Title = "Conferma eliminazione",
                Content = $"Eliminare l'intervento del {intervention.DisplayDate} per la stanza {intervention.Room}?",
                PrimaryButtonText = "Elimina",
                CloseButtonText = "Annulla",
                XamlRoot = this.XamlRoot
            };
            var result = await confirm.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await DataService.Instance.DeleteInterventionAsync(intervention);
                LoadInterventions();
            }
        }
    }

    private async void ExportTxt_Click(object sender, RoutedEventArgs e)
    {
        var interventions = DataService.Instance.Interventions
            .OrderByDescending(i => i.Timestamp)
            .ToList();

        var lines = new List<string>
        {
            "=== TUTTI GLI INTERVENTI - THE REGENCY MAINTENANCE ===",
            $"Esportato: {DateHelper.NowFormatted}",
            $"Totale: {interventions.Count}",
            string.Empty
        };

        foreach (var inv in interventions)
            lines.Add($"{inv.DisplayDate} {inv.DisplayTime} | {inv.Room} | {inv.Category} | {inv.Note} | {inv.Operator}");

        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"interventi_{DateHelper.TimestampFile}.txt");
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

    private async void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        var backupService = new BackupService();
        var path = await backupService.CreateBackupAsync();

        var dlg = new ContentDialog
        {
            Title = "Backup Creato",
            Content = $"Backup salvato in:\n{path}",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dlg.ShowAsync();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadInterventions();
    }
}
