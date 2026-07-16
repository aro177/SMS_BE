namespace Student_Management_System.Common.DateTimes;

public static class DateTimeUtc
{
    private static readonly Lazy<TimeZoneInfo> VietnamTimeZone = new(GetVietnamTimeZone);

    public static DateOnly TodayInVietnam()
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone.Value));
    }

    public static DateTime FromVietnamLocal(DateOnly date, TimeOnly time)
    {
        return TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(time), VietnamTimeZone.Value);
    }

    public static DateTime? Normalize(DateTime? value)
    {
        return value is null ? null : Normalize(value.Value);
    }

    public static DateTime Normalize(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => TimeZoneInfo.ConvertTimeToUtc(value, VietnamTimeZone.Value)
        };
    }

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }
}
