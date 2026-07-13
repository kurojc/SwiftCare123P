using SwiftCare123P.Models;

namespace SwiftCare123P.Services;

/// <summary>
/// Abstraction over the database. ViewModels depend on this interface, not on
/// SQLite directly — if you ever swapped SQLite for a REST API, only
/// DatabaseService.cs would need to change; the ViewModels wouldn't.
/// </summary>
public interface IDatabaseService
{
    Task InitializeAsync();
    Task<int> SaveUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}
