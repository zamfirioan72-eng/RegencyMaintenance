using System.Text.Json.Serialization;

namespace RegencyMaintenance.Models;

public class RoomFilterData
{
    [JsonPropertyName("last_cleaning")]
    public string LastCleaning { get; set; } = string.Empty;

    [JsonPropertyName("next_cleaning")]
    public string NextCleaning { get; set; } = string.Empty;
}

public class HydronicFilterData
{
    [JsonPropertyName("last_maintenance")]
    public string LastMaintenance { get; set; } = string.Empty;

    [JsonPropertyName("next_maintenance")]
    public string NextMaintenance { get; set; } = string.Empty;
}

public class BatteryData
{
    [JsonPropertyName("last_change")]
    public string LastChange { get; set; } = string.Empty;

    [JsonPropertyName("next_change")]
    public string NextChange { get; set; } = string.Empty;
}

public class BathroomFaucetData
{
    [JsonPropertyName("last_maintenance")]
    public string LastMaintenance { get; set; } = string.Empty;

    [JsonPropertyName("next_maintenance")]
    public string NextMaintenance { get; set; } = string.Empty;
}
