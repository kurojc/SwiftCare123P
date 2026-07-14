using SQLite;

namespace SwiftCare123P.Models;

public class ReviewEntity
{
    [PrimaryKey, AutoIncrement]
    public int ReviewID { get; set; }

    public int BookingID { get; set; }
    public int UserID { get; set; }
    public int CaregiverID { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
}