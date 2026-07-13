using SQLite;

namespace SwiftCare123P.Models;

/// <summary>
/// Represents a row in the Users table. Plain data only — no logic.
/// The [PrimaryKey]/[AutoIncrement]/[Unique] attributes come from sqlite-net-pcl
/// and tell it how to build the table's schema.
/// </summary>
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    [Unique]
    public string Email { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;
    public DateTime Birthdate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // NOTE: stored as plain text for simplicity in this school project.
    // In a real app this should be hashed (e.g. with BCrypt) before saving.
    public string Password { get; set; } = string.Empty;

    // "CareSeeker" or "Caregiver" — set from the sign-up role toggle.
    public string Role { get; set; } = string.Empty;

    public string AccountStatus { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
