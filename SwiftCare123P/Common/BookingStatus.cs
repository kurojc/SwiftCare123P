namespace SwiftCare123P.Common;

/// <summary>
/// Single source of truth for booking status strings. These are used across the
/// database layer, view models, and XAML data triggers/filters — centralizing them
/// here avoids typo-driven mismatches between e.g. a filter switch and the value
/// actually written to the database.
/// </summary>
public static class BookingStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}
