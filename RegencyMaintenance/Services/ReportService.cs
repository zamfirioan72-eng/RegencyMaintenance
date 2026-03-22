using RegencyMaintenance.Helpers;
using RegencyMaintenance.Models;

namespace RegencyMaintenance.Services;

public class ReportService
{
    private readonly DataService _dataService;

    public ReportService()
    {
        _dataService = DataService.Instance;
    }

    public async Task<string> GenerateDailyReportAsync()
    {
        var today = DateHelper.TodayIso;
        var lines = new List<string>();

        lines.Add("╔══════════════════════════════════════════════════════════════╗");
        lines.Add("║          THE REGENCY MAINTENANCE - REPORT GIORNALIERO        ║");
        lines.Add("╚══════════════════════════════════════════════════════════════╝");
        lines.Add(string.Empty);
        lines.Add($"Data: {DateHelper.TodayFormatted} - {DateHelper.GetDayOfWeekItalian()}");
        lines.Add($"Generato: {DateHelper.NowFormatted}");
        lines.Add($"Operatore: {_dataService.CurrentOperator}");
        lines.Add(string.Empty);

        // Preventive maintenance section
        lines.Add("══════════════════════════════════════════════════════════════");
        lines.Add("  MANUTENZIONI PREVENTIVE");
        lines.Add("══════════════════════════════════════════════════════════════");

        // Room filters
        lines.Add(string.Empty);
        lines.Add("▶ FILTRI ARIA CONDIZIONATA (ciclo 20 giorni):");
        var filterAlerts = new List<string>();
        foreach (var room in DataService.GetAllFilterRooms())
        {
            if (_dataService.RoomFilterDates.TryGetValue(room, out var fd))
            {
                var days = DateHelper.DaysSince(fd.LastCleaning);
                if (days >= 20)
                    filterAlerts.Add($"  ⚠ Stanza {room}: {days} giorni dall'ultima pulizia ({DateHelper.IsoToItalian(fd.LastCleaning)}) - DA PULIRE");
                else if (days >= 15)
                    filterAlerts.Add($"  ⚡ Stanza {room}: {days} giorni dall'ultima pulizia ({DateHelper.IsoToItalian(fd.LastCleaning)}) - TRA POCO");
            }
            else
            {
                filterAlerts.Add($"  ? Stanza {room}: mai registrata");
            }
        }
        if (filterAlerts.Count == 0)
            lines.Add("  ✓ Tutti i filtri sono in regola");
        else
            lines.AddRange(filterAlerts);

        // Hydronic filters
        lines.Add(string.Empty);
        lines.Add("▶ FILTRI Y MODULI IDRONICI (ciclo 6 mesi):");
        foreach (var comp in DataService.GetHydronicComponents())
        {
            if (_dataService.HydronicFilterDates.TryGetValue(comp, out var hd))
            {
                var months = DateHelper.MonthsBetween(hd.LastMaintenance);
                var status = months >= 6 ? "DA SOSTITUIRE" : months >= 4 ? "TRA POCO" : "OK";
                lines.Add($"  {comp}: {months:F1} mesi ({DateHelper.IsoToItalian(hd.LastMaintenance)}) - {status}");
            }
            else
            {
                lines.Add($"  {comp}: mai registrato");
            }
        }

        // Batteries
        lines.Add(string.Empty);
        lines.Add("▶ BATTERIE PORTE STANZE (ciclo 365 giorni):");
        var batteryAlerts = new List<string>();
        foreach (var room in DataService.GetBatteryRooms())
        {
            if (_dataService.BatteryDates.TryGetValue(room, out var bd))
            {
                var days = DateHelper.DaysSince(bd.LastChange);
                if (days >= 365)
                    batteryAlerts.Add($"  ⚠ Stanza {room}: {days} giorni ({DateHelper.IsoToItalian(bd.LastChange)}) - DA SOSTITUIRE");
                else if (days >= 300)
                    batteryAlerts.Add($"  ⚡ Stanza {room}: {days} giorni ({DateHelper.IsoToItalian(bd.LastChange)}) - TRA POCO");
            }
        }
        if (batteryAlerts.Count == 0)
            lines.Add("  ✓ Tutte le batterie sono in regola");
        else
            lines.AddRange(batteryAlerts);

        // Bathroom faucets
        lines.Add(string.Empty);
        lines.Add("▶ ROMPIGETTO BAGNO (ciclo 180 giorni):");
        var faucetAlerts = new List<string>();
        foreach (var room in DataService.GetAllFilterRooms())
        {
            if (_dataService.BathroomFaucetDates.TryGetValue(room, out var bfd))
            {
                var days = DateHelper.DaysSince(bfd.LastMaintenance);
                if (days >= 180)
                    faucetAlerts.Add($"  ⚠ Stanza {room}: {days} giorni ({DateHelper.IsoToItalian(bfd.LastMaintenance)}) - DA SOSTITUIRE");
                else if (days >= 150)
                    faucetAlerts.Add($"  ⚡ Stanza {room}: {days} giorni ({DateHelper.IsoToItalian(bfd.LastMaintenance)}) - TRA POCO");
            }
        }
        if (faucetAlerts.Count == 0)
            lines.Add("  ✓ Tutti i rompigetto sono in regola");
        else
            lines.AddRange(faucetAlerts);

        // Ordinary interventions today
        lines.Add(string.Empty);
        lines.Add("══════════════════════════════════════════════════════════════");
        lines.Add("  INTERVENTI ORDINARI OGGI");
        lines.Add("══════════════════════════════════════════════════════════════");

        var todayInterventions = _dataService.Interventions
            .Where(i => i.Date == today)
            .OrderBy(i => i.Room)
            .ToList();

        if (todayInterventions.Count == 0)
        {
            lines.Add("  Nessun intervento registrato oggi");
        }
        else
        {
            var byRoom = todayInterventions.GroupBy(i => i.Room);
            foreach (var group in byRoom)
            {
                lines.Add(string.Empty);
                lines.Add($"  📍 {group.Key}:");
                foreach (var inv in group)
                {
                    lines.Add($"     [{inv.DisplayTime}] {inv.Category}");
                    if (!string.IsNullOrEmpty(inv.Note))
                        lines.Add($"     Note: {inv.Note}");
                    lines.Add($"     Operatore: {inv.Operator}");
                }
            }
        }

        // Statistics
        lines.Add(string.Empty);
        lines.Add("══════════════════════════════════════════════════════════════");
        lines.Add("  RIEPILOGO STATISTICHE");
        lines.Add("══════════════════════════════════════════════════════════════");
        lines.Add($"  Interventi oggi: {todayInterventions.Count}");
        lines.Add($"  Interventi totali: {_dataService.Interventions.Count}");
        lines.Add(string.Empty);
        lines.Add("══════════════════════════════════════════════════════════════");
        lines.Add("  Fine Report");
        lines.Add("══════════════════════════════════════════════════════════════");

        var reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
        Directory.CreateDirectory(reportsDir);
        var reportPath = Path.Combine(reportsDir, $"report_{DateHelper.TimestampFile}.txt");
        await File.WriteAllLinesAsync(reportPath, lines);

        return reportPath;
    }

    public List<Intervention> GetTodayFilterInterventions()
    {
        var today = DateHelper.TodayIso;
        return _dataService.Interventions
            .Where(i => i.Date == today &&
                (i.Category.Contains("FILTRO") || i.Category.Contains("BATTERIA") ||
                 i.Category.Contains("ROMPIGETTO") || i.Category.Contains("IDRONICO")))
            .ToList();
    }
}
