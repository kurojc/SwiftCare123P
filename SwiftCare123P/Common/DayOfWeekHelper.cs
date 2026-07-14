namespace SwiftCare123P.Common;

/// <summary>
/// Single source of truth for the app's day-of-week convention (matches the original
/// web app and the Availability screen): index 0 = Monday ... 6 = Sunday.
///
/// .NET's built-in <see cref="DayOfWeek"/> enum uses a different convention
/// (0 = Sunday ... 6 = Saturday), so any code that needs to compare a real
/// <see cref="DateTime"/> against a caregiver's stored AvailableDays string
/// MUST go through <see cref="ToAppIndex"/> rather than re-deriving the mapping
/// locally. Keeping this logic in one place avoids the app silently drifting
/// out of sync with itself (as the mismatched comments in the codebase show).
/// </summary>
public static class DayOfWeekHelper
{
    /// <summary>Display abbreviations, in app order (index 0 = Monday).</summary>
    public static readonly string[] Abbreviations = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    /// <summary>Full display names, in app order (index 0 = Monday).</summary>
    public static readonly string[] FullNames =
        { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    /// <summary>Converts a .NET <see cref="DayOfWeek"/> into the app's Mon=0..Sun=6 index.</summary>
    public static int ToAppIndex(DayOfWeek dayOfWeek) => ((int)dayOfWeek + 6) % 7;

    /// <summary>Parses a caregiver's stored "0,2,4"-style AvailableDays string into a set of app-order indices.</summary>
    public static HashSet<int> ParseAvailableDays(string? csv) =>
        (csv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var i) ? i : -1)
            .Where(i => i is >= 0 and <= 6)
            .ToHashSet();

    /// <summary>Builds the "0,2,4"-style CSV string from a 7-length selected/unselected list, in app order.</summary>
    public static string ToCsv(IEnumerable<bool> selectedByIndex) =>
        string.Join(",", selectedByIndex
            .Select((selected, index) => selected ? index.ToString() : null)
            .Where(x => x is not null));

    /// <summary>Human-readable summary of a set of app-order day indices, e.g. "Mon, Wed, Fri".</summary>
    public static string Describe(IEnumerable<int> appIndices)
    {
        var ordered = appIndices.Where(i => i is >= 0 and <= 6).Distinct().OrderBy(i => i);
        var text = string.Join(", ", ordered.Select(i => Abbreviations[i]));
        return string.IsNullOrEmpty(text) ? "No days set" : text;
    }
}
