namespace SwiftCare123P.Services;

/// <summary>
/// Holds one shared IDatabaseService instance for the whole app.
/// Kept intentionally simple (no full DI container) so it's easy to follow —
/// ViewModels still depend on the IDatabaseService interface, they just get
/// their instance from here by default instead of "new DatabaseService()"
/// being scattered everywhere.
/// </summary>
public static class AppServices
{
    public static IDatabaseService Database { get; } = new DatabaseService();
}
