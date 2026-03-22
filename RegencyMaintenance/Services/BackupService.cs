using System.IO.Compression;
using System.Text.Json;
using RegencyMaintenance.Helpers;
using RegencyMaintenance.Models;

namespace RegencyMaintenance.Services;

public class BackupService
{
    private readonly string _backupDir;
    private readonly string _dataFilePath;

    public BackupService()
    {
        _backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "manutenzione_data.json");
        Directory.CreateDirectory(_backupDir);
    }

    public async Task<string> CreateBackupAsync()
    {
        var timestamp = DateHelper.TimestampFile;
        var backupName = $"backup_regency_{timestamp}.zip";
        var backupPath = Path.Combine(_backupDir, backupName);

        await Task.Run(() =>
        {
            using var zip = ZipFile.Open(backupPath, ZipArchiveMode.Create);

            // Add main data file
            if (File.Exists(_dataFilePath))
                zip.CreateEntryFromFile(_dataFilePath, "data/manutenzione_data.json");

            // Add reports if they exist
            var reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
            if (Directory.Exists(reportsDir))
            {
                foreach (var report in Directory.GetFiles(reportsDir, "*.txt"))
                    zip.CreateEntryFromFile(report, $"reports/{Path.GetFileName(report)}");
            }

            // Create backup_info.json
            var info = new BackupInfo
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Version = "1.0",
                FilesCopied = 1,
                BackupSize = File.Exists(_dataFilePath) ? new FileInfo(_dataFilePath).Length : 0,
                System = new SystemInfo
                {
                    Platform = Environment.OSVersion.ToString(),
                    DotnetVersion = Environment.Version.ToString(),
                    WinUIVersion = "1.5"
                }
            };
            var infoJson = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
            var infoEntry = zip.CreateEntry("config/backup_info.json");
            using var writer = new StreamWriter(infoEntry.Open());
            writer.Write(infoJson);
        });

        return backupPath;
    }

    public async Task RestoreBackupAsync(string backupPath)
    {
        // Create emergency backup first
        if (File.Exists(_dataFilePath))
        {
            var emergencyDir = Path.Combine(_backupDir, "emergency");
            Directory.CreateDirectory(emergencyDir);
            File.Copy(_dataFilePath,
                Path.Combine(emergencyDir, $"emergency_{DateHelper.TimestampFile}.json"), true);
        }

        await Task.Run(() =>
        {
            using var zip = ZipFile.OpenRead(backupPath);
            var dataEntry = zip.GetEntry("data/manutenzione_data.json");
            if (dataEntry != null)
            {
                using var stream = dataEntry.Open();
                using var fileStream = File.Create(_dataFilePath);
                stream.CopyTo(fileStream);
            }
        });
    }

    public List<BackupEntry> ListBackups()
    {
        if (!Directory.Exists(_backupDir))
            return new List<BackupEntry>();

        return Directory.GetFiles(_backupDir, "backup_*.zip")
            .Select(f =>
            {
                var fi = new FileInfo(f);
                return new BackupEntry
                {
                    Filename = fi.Name,
                    Path = fi.FullName,
                    Date = fi.CreationTime.ToString("dd/MM/yyyy HH:mm"),
                    Size = fi.Length
                };
            })
            .OrderByDescending(b => b.Date)
            .ToList();
    }

    public BackupStats GetBackupStats()
    {
        var backups = ListBackups();
        if (backups.Count == 0)
            return new BackupStats();

        return new BackupStats
        {
            Total = backups.Count,
            TotalSize = backups.Sum(b => b.Size),
            Oldest = backups.Min(b => b.Date) ?? string.Empty,
            Newest = backups.Max(b => b.Date) ?? string.Empty
        };
    }

    public async Task<BackupInfo?> GetBackupInfoAsync(string backupPath)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var zip = ZipFile.OpenRead(backupPath);
                var infoEntry = zip.GetEntry("config/backup_info.json");
                if (infoEntry == null) return null;
                using var reader = new StreamReader(infoEntry.Open());
                var json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<BackupInfo>(json);
            });
        }
        catch (Exception ex)
        {
            // Log and surface error to caller - corrupted or unreadable backup info
            System.Diagnostics.Debug.WriteLine($"BackupInfo read error for '{backupPath}': {ex.Message}");
            return null;
        }
    }

    public async Task DeleteBackupAsync(string backupPath)
    {
        await Task.Run(() => File.Delete(backupPath));
    }

    public async Task<string> ExportBackupListAsync()
    {
        var backups = ListBackups();
        var reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
        Directory.CreateDirectory(reportsDir);
        var outputPath = Path.Combine(reportsDir, $"lista_backup_{DateHelper.TimestampFile}.txt");

        var lines = new List<string>
        {
            "=== LISTA BACKUP REGENCY MAINTENANCE ===",
            $"Generato: {DateHelper.NowFormatted}",
            $"Totale backup: {backups.Count}",
            string.Empty
        };

        foreach (var b in backups)
            lines.Add($"{b.Date} | {b.Filename} | {b.SizeMB:F2} MB | {b.Path}");

        await File.WriteAllLinesAsync(outputPath, lines);
        return outputPath;
    }
}
