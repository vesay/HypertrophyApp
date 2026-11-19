namespace HypertrophyApp.Application.Common;

internal static class DateRules
{
    private static readonly DateOnly Earliest = new(2000, 1, 1);

    /// <summary>
    /// Logs use the visitor's local calendar date, which can be one day ahead of UTC,
    /// so "tomorrow (UTC)" is still accepted.
    /// </summary>
    public static string? Validate(DateOnly date, TimeProvider clock)
    {
        var latest = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime).AddDays(1);
        return date < Earliest || date > latest ? "You can't log entries for that date." : null;
    }
}
