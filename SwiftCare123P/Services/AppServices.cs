namespace SwiftCare123P.Services;

public static class AppServices
{
    private static IDatabaseService? _database;

    public static IDatabaseService Database
    {
        get
        {
            _database ??= new DatabaseService();
            return _database;
        }
    }

    public static void SetDatabase(IDatabaseService databaseService)
    {
        _database = databaseService;
    }
}
