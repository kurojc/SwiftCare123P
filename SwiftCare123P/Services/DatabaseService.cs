using SQLite;
using SwiftCare123P.Models;

namespace SwiftCare123P.Services;

/// <summary>
/// Concrete implementation of IDatabaseService using sqlite-net-pcl.
/// Owns the connection and table creation — nothing outside this class
/// should ever touch SQLiteAsyncConnection directly.
/// </summary>
public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task EnsureInitializedAsync()
    {
        if (_database is not null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "swiftcare.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<User>();
    }

    public Task InitializeAsync() => EnsureInitializedAsync();

    public async Task<int> SaveUserAsync(User user)
    {
        await EnsureInitializedAsync();
        return await _database!.InsertAsync(user);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await EnsureInitializedAsync();
        return await _database!.Table<User>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var existing = await GetUserByEmailAsync(email);
        return existing is not null;
    }
}
