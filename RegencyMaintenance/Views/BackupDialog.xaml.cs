using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegencyMaintenance.Models;
using RegencyMaintenance.Services;

namespace RegencyMaintenance.Views;

public sealed partial class BackupDialog : ContentDialog
{
    private readonly BackupService _backupService;

    public BackupDialog()
    {
        this.InitializeComponent();
        _backupService = new BackupService();
        this.Loaded += (s, e) => LoadBackups();
    }

    private void LoadBackups()
    {
        var backups = _backupService.ListBackups();
        BackupList.ItemsSource = backups;

        var stats = _backupService.GetBackupStats();
        TotalBackupsText.Text = stats.Total.ToString();
        TotalSizeText.Text = $"{stats.TotalSizeMB:F2} MB";
        NewestText.Text = stats.Newest;
        OldestText.Text = stats.Oldest;
    }

    private async void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = await _backupService.CreateBackupAsync();
            var dlg = new ContentDialog
            {
                Title = "Backup Creato",
                Content = $"Backup salvato in:\n{path}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dlg.ShowAsync();
            LoadBackups();
        }
        catch (Exception ex)
        {
            var dlg = new ContentDialog
            {
                Title = "Errore",
                Content = $"Errore durante il backup:\n{ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dlg.ShowAsync();
        }
    }

    private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        if (BackupList.SelectedItem is not BackupEntry selected)
        {
            var info = new ContentDialog
            {
                Title = "Seleziona un backup",
                Content = "Seleziona un backup dalla lista prima di ripristinare.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await info.ShowAsync();
            return;
        }

        var confirm = new ContentDialog
        {
            Title = "Conferma ripristino",
            Content = $"Ripristinare il backup:\n{selected.Filename}\n\nI dati attuali saranno sostituiti (verrà creato un backup di emergenza).",
            PrimaryButtonText = "Ripristina",
            CloseButtonText = "Annulla",
            XamlRoot = this.XamlRoot
        };
        var result = await confirm.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                await _backupService.RestoreBackupAsync(selected.Path);
                await DataService.Instance.LoadAsync();

                var dlg = new ContentDialog
                {
                    Title = "Ripristinato",
                    Content = "Backup ripristinato con successo.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dlg.ShowAsync();
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog
                {
                    Title = "Errore",
                    Content = $"Errore durante il ripristino:\n{ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dlg.ShowAsync();
            }
        }
    }

    private async void DeleteBackup_Click(object sender, RoutedEventArgs e)
    {
        if (BackupList.SelectedItem is not BackupEntry selected)
        {
            var info = new ContentDialog
            {
                Title = "Seleziona un backup",
                Content = "Seleziona un backup dalla lista prima di eliminare.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await info.ShowAsync();
            return;
        }

        var confirm = new ContentDialog
        {
            Title = "Conferma eliminazione",
            Content = $"Eliminare il backup:\n{selected.Filename}?",
            PrimaryButtonText = "Elimina",
            CloseButtonText = "Annulla",
            XamlRoot = this.XamlRoot
        };
        var result = await confirm.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await _backupService.DeleteBackupAsync(selected.Path);
            LoadBackups();
        }
    }

    private async void ExportList_Click(object sender, RoutedEventArgs e)
    {
        var path = await _backupService.ExportBackupListAsync();
        var dlg = new ContentDialog
        {
            Title = "Esportato",
            Content = $"Lista backup salvata in:\n{path}",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dlg.ShowAsync();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadBackups();
    }
}
