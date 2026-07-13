namespace SwiftCare123P.Models;

public class UserModel
{
    public int UserID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? ContactNo { get; set; }
    public string? Gender { get; set; }
    public DateTime? Birthdate { get; set; }
    public string? Address { get; set; }
}