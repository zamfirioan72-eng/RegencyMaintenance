using System.Text.Json;
using RegencyMaintenance.Helpers;
using RegencyMaintenance.Models;

namespace RegencyMaintenance.Services;

public class DataService
{
    private static DataService? _instance;
    public static DataService Instance => _instance ??= new DataService();

    private const string DataFileName = "manutenzione_data.json";
    private readonly string _dataFilePath;
    private AppData _data = new();

    public string CurrentOperator { get; set; } = string.Empty;
    public List<Intervention> Interventions => _data.Interventions;
    public Dictionary<string, RoomFilterData> RoomFilterDates => _data.RoomFilterDates;
    public Dictionary<string, RoomFilterData> CommonFilterDates => _data.CommonFilterDates;
    public Dictionary<string, HydronicFilterData> HydronicFilterDates => _data.HydronicFilterDates;
    public Dictionary<string, BatteryData> BatteryDates => _data.BatteryDates;
    public Dictionary<string, BathroomFaucetData> BathroomFaucetDates => _data.BathroomFaucetDates;
    public string LastOperator => _data.LastOperator;

    private DataService()
    {
        _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataFileName);
    }

    public async Task LoadAsync()
    {
        if (File.Exists(_dataFilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                _data = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            }
            catch
            {
                _data = new AppData();
            }
        }
        else
        {
            _data = new AppData();
        }
    }

    public async Task SaveAsync()
    {
        // Auto backup before overwrite
        if (File.Exists(_dataFilePath))
        {
            var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups", "auto");
            Directory.CreateDirectory(backupDir);
            var backupFile = Path.Combine(backupDir, $"auto_{DateHelper.TimestampFile}.json");
            File.Copy(_dataFilePath, backupFile, true);

            // Keep only last 5 auto backups
            var autoBackups = Directory.GetFiles(backupDir, "auto_*.json")
                .OrderBy(f => f).ToList();
            while (autoBackups.Count > 5)
            {
                File.Delete(autoBackups[0]);
                autoBackups.RemoveAt(0);
            }
        }

        _data.LastSave = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        _data.LastOperator = CurrentOperator;

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_data, options);
        await File.WriteAllTextAsync(_dataFilePath, json);
    }

    public async Task AddInterventionAsync(string room, string category, string note)
    {
        var now = DateTime.Now;
        var intervention = new Intervention
        {
            Room = room,
            Category = category,
            Note = note,
            Timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss"),
            Date = now.ToString("yyyy-MM-dd"),
            Operator = CurrentOperator
        };
        _data.Interventions.Add(intervention);
        await SaveAsync();
    }

    public async Task DeleteInterventionAsync(Intervention intervention)
    {
        _data.Interventions.Remove(intervention);
        await SaveAsync();
    }

    public async Task SetRoomFilterDateAsync(string room, string isoDate)
    {
        _data.RoomFilterDates[room] = new RoomFilterData
        {
            LastCleaning = isoDate,
            NextCleaning = DateHelper.AddDays(isoDate, 20)
        };
        await SaveAsync();
    }

    public async Task SetCommonFilterDateAsync(string room, string isoDate)
    {
        _data.CommonFilterDates[room] = new RoomFilterData
        {
            LastCleaning = isoDate,
            NextCleaning = DateHelper.AddDays(isoDate, 20)
        };
        await SaveAsync();
    }

    public async Task SetHydronicFilterDateAsync(string component, string isoDate)
    {
        _data.HydronicFilterDates[component] = new HydronicFilterData
        {
            LastMaintenance = isoDate,
            NextMaintenance = DateHelper.AddMonths(isoDate, 6)
        };
        await SaveAsync();
    }

    public async Task SetBatteryDateAsync(string room, string isoDate)
    {
        _data.BatteryDates[room] = new BatteryData
        {
            LastChange = isoDate,
            NextChange = DateHelper.AddDays(isoDate, 365)
        };
        await SaveAsync();
    }

    public async Task SetBathroomFaucetDateAsync(string room, string isoDate)
    {
        _data.BathroomFaucetDates[room] = new BathroomFaucetData
        {
            LastMaintenance = isoDate,
            NextMaintenance = DateHelper.AddDays(isoDate, 180)
        };
        await SaveAsync();
    }

    public static List<string> GetFloorRooms(int floor)
    {
        return floor switch
        {
            1 => Enumerable.Range(101, 8).Select(r => r.ToString()).ToList(),
            2 => Enumerable.Range(201, 8).Select(r => r.ToString()).ToList(),
            3 => Enumerable.Range(301, 8).Select(r => r.ToString()).ToList(),
            4 => Enumerable.Range(401, 8).Select(r => r.ToString()).ToList(),
            5 => Enumerable.Range(501, 6).Select(r => r.ToString()).ToList(),
            6 => Enumerable.Range(601, 3).Select(r => r.ToString()).ToList(),
            _ => new List<string>()
        };
    }

    public static List<string> GetAllRoomNames()
    {
        var rooms = new List<string>();
        for (int floor = 1; floor <= 6; floor++)
            rooms.AddRange(GetFloorRooms(floor));
        rooms.Add("SPAZIO COMUNE");
        return rooms;
    }

    public static List<string> GetHydronicComponents()
    {
        return new List<string>
        {
            "FILTRO Y MODULO IDRONICO 1.RIGA",
            "FILTRO Y MODULO IDRONICO 2.RIGA",
            "FILTRO Y MODULO IDRONICO 3.RIGA",
            "FILTRO MODULO IDRONICO 4.RIGA",
            "FILTRO Y POMPA RICIRCOLO"
        };
    }

    public static List<string> GetBatteryRooms()
    {
        var rooms = new List<string>();
        for (int floor = 1; floor <= 6; floor++)
        {
            var floorRooms = GetFloorRooms(floor);
            rooms.AddRange(floorRooms);
            // Add double rooms
            if (floor == 1) rooms.Add("102-103");
            if (floor == 2) rooms.Add("202-203");
            if (floor == 3) rooms.Add("302-303");
            if (floor == 4) rooms.Add("402-403");
        }
        return rooms.Distinct().OrderBy(r => r).ToList();
    }

    public static List<string> GetAllFilterRooms()
    {
        var rooms = new List<string>();
        for (int floor = 1; floor <= 6; floor++)
            rooms.AddRange(GetFloorRooms(floor));
        return rooms;
    }
}
