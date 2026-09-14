using Cronos;
using Hangfire;

namespace RetroHiscore.Api.Features.Sync;

public static class SyncRecurringJobCron
{
    public static string ForMinuteInterval(int intervalMinutes, int minMinutes, int maxMinutes)
    {
        var clamped = Math.Clamp(intervalMinutes, minMinutes, maxMinutes);
        if (clamped <= 59)
        {
            var cron = Cron.MinuteInterval(clamped);
            Validate(cron);
            return cron;
        }

        if (clamped >= 60 * 24)
        {
            var daily = Cron.Daily();
            Validate(daily);
            return daily;
        }

        var hours = Math.Clamp((clamped + 59) / 60, 1, 23);
        var hourly = $"0 */{hours} * * *";
        Validate(hourly);
        return hourly;
    }

    public static string ForIntervalDays(int intervalDays)
    {
        var days = Math.Clamp(intervalDays, 1, 365);
        var cron = days == 1 ? Cron.Daily() : $"0 0 */{days} * *";
        Validate(cron);
        return cron;
    }

    public static void Validate(string cron)
    {
        _ = CronExpression.Parse(cron, CronFormat.Standard);
    }
}
