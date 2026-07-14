using SQLite;
using SwiftCare123P.Common;

namespace SwiftCare123P.Models;

public class BookingEntity
{
    [PrimaryKey, AutoIncrement]
    public int BookingID { get; set; }

    public int UserID { get; set; }        
    public int CaregiverID { get; set; }    
    public int ServiceID { get; set; }

    public DateTime BookingDate { get; set; }
    public string StartTime { get; set; } = string.Empty; // "HH:mm"
    public string EndTime { get; set; } = string.Empty;   // "HH:mm"

    public string Status { get; set; } = BookingStatus.Pending;

    public decimal TotalPrice { get; set; }
}