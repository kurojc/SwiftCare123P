using SwiftCare123P.Models;

namespace SwiftCare123P.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task SaveUserAsync(User user);
}
