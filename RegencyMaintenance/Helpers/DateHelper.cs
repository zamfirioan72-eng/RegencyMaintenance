namespace RegencyMaintenance.Helpers;

public static class DateHelper
{
    public static string TodayFormatted => DateTime.Today.ToString("dd/MM/yyyy");
    public static string NowFormatted => DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    public static string TodayIso => DateTime.Today.ToString("yyyy-MM-dd");
    public static string TimestampFile => DateTime.Now.ToString("yyyyMMdd_HHmmss");

    public static string GetDayOfWeekItalian()
    {
        return DateTime.Today.DayOfWeek switch
        {
            DayOfWeek.Monday => "Lunedì",
            DayOfWeek.Tuesday => "Martedì",
            DayOfWeek.Wednesday => "Mercoledì",
            DayOfWeek.Thursday => "Giovedì",
            DayOfWeek.Friday => "Venerdì",
            DayOfWeek.Saturday => "Sabato",
            DayOfWeek.Sunday => "Domenica",
            _ => string.Empty
        };
    }

    public static bool TryParseItalianDate(string dateStr, out DateTime result)
    {
        return DateTime.TryParseExact(dateStr, "dd/MM/yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out result);
    }

    public static bool TryParseIsoDate(string dateStr, out DateTime result)
    {
        return DateTime.TryParseExact(dateStr, "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out result);
    }

    public static int DaysSince(string isoDateStr)
    {
        if (TryParseIsoDate(isoDateStr, out var dt))
            return (DateTime.Today - dt).Days;
        return 0;
    }

    public static double MonthsBetween(string isoDateStr)
    {
        if (TryParseIsoDate(isoDateStr, out var dt))
            return (DateTime.Today - dt).Days / 30.0;
        return 0;
    }

    public static string AddDays(string isoDateStr, int days)
    {
        if (TryParseIsoDate(isoDateStr, out var dt))
            return dt.AddDays(days).ToString("yyyy-MM-dd");
        return isoDateStr;
    }

    public static string AddMonths(string isoDateStr, int months)
    {
        if (TryParseIsoDate(isoDateStr, out var dt))
            return dt.AddMonths(months).ToString("yyyy-MM-dd");
        return isoDateStr;
    }

    public static string IsoToItalian(string isoDateStr)
    {
        if (TryParseIsoDate(isoDateStr, out var dt))
            return dt.ToString("dd/MM/yyyy");
        return isoDateStr;
    }

    public static string ItalianToIso(string italianDateStr)
    {
        if (TryParseItalianDate(italianDateStr, out var dt))
            return dt.ToString("yyyy-MM-dd");
        return italianDateStr;
    }
}
