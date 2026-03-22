using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegencyMaintenance.Services;

namespace RegencyMaintenance.Views;

public sealed partial class ReportPreviewDialog : ContentDialog
{
    private readonly string _reportContent;
    private readonly string _reportPath;

    public ReportPreviewDialog(string content, string path)
    {
        this.InitializeComponent();
        _reportContent = content;
        _reportPath = path;
        this.Loaded += (s, e) => ReportTextBox.Text = _reportContent;
    }

    private async void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = _reportPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            var dlg = new ContentDialog
            {
                Title = "Errore apertura file",
                Content = $"Impossibile aprire il file:\n{ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dlg.ShowAsync();
        }
    }

    private async void CopyText_Click(object sender, RoutedEventArgs e)
    {
        var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
        dataPackage.SetText(_reportContent);
        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);

        var dlg = new ContentDialog
        {
            Title = "Copiato",
            Content = "Testo copiato negli appunti.",
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
}
