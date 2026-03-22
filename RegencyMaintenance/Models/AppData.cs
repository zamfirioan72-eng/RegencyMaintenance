using System.Text.Json.Serialization;

namespace RegencyMaintenance.Models;

public class AppData
{
    [JsonPropertyName("interventions")]
    public List<Intervention> Interventions { get; set; } = new();

    [JsonPropertyName("room_filter_dates")]
    public Dictionary<string, RoomFilterData> RoomFilterDates { get; set; } = new();

    [JsonPropertyName("common_filter_dates")]
    public Dictionary<string, RoomFilterData> CommonFilterDates { get; set; } = new();

    [JsonPropertyName("hydronic_filter_dates")]
    public Dictionary<string, HydronicFilterData> HydronicFilterDates { get; set; } = new();

    [JsonPropertyName("battery_dates")]
    public Dictionary<string, BatteryData> BatteryDates { get; set; } = new();

    [JsonPropertyName("bathroom_faucet_dates")]
    public Dictionary<string, BathroomFaucetData> BathroomFaucetDates { get; set; } = new();

    [JsonPropertyName("last_save")]
    public string LastSave { get; set; } = string.Empty;

    [JsonPropertyName("last_operator")]
    public string LastOperator { get; set; } = string.Empty;
}
