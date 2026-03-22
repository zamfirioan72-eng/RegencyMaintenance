using System.Text.Json.Serialization;

namespace RegencyMaintenance.Models;

public class SystemInfo
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("dotnet_version")]
    public string DotnetVersion { get; set; } = string.Empty;

    [JsonPropertyName("winui_version")]
    public string WinUIVersion { get; set; } = string.Empty;
}

public class BackupInfo
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("files_copied")]
    public int FilesCopied { get; set; }

    [JsonPropertyName("backup_size")]
    public long BackupSize { get; set; }

    [JsonPropertyName("system")]
    public SystemInfo System { get; set; } = new();
}

public class BackupEntry
{
    public string Filename { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public long Size { get; set; }
    public double SizeMB => Math.Round(Size / 1024.0 / 1024.0, 2);
}

public class BackupStats
{
    public int Total { get; set; }
    public long TotalSize { get; set; }
    public double TotalSizeMB => Math.Round(TotalSize / 1024.0 / 1024.0, 2);
    public string Oldest { get; set; } = string.Empty;
    public string Newest { get; set; } = string.Empty;
}
