using System.Text.Json.Serialization;

namespace RegencyMaintenance.Models;

public class Intervention
{
    [JsonPropertyName("room")]
    public string Room { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = string.Empty;

    [JsonIgnore]
    public string DisplayDate
    {
        get
        {
            if (DateTime.TryParseExact(Date, "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return Date;
        }
    }

    [JsonIgnore]
    public string DisplayTime
    {
        get
        {
            if (!string.IsNullOrEmpty(Timestamp) && Timestamp.Length >= 19)
                return Timestamp.Substring(11, 8);
            return string.Empty;
        }
    }
}
