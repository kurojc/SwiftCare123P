using SQLite;

namespace SwiftCare123P.Models;


public class CaregiverProfileEntity
{
    [PrimaryKey, AutoIncrement]
    public int CaregiverID { get; set; }

    public int UserID { get; set; }

    public string ServicesOffered { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string AvailabilityStatus { get; set; } = "Unavailable";
    public string Bio { get; set; } = string.Empty;

    public string AvailableDays { get; set; } = string.Empty;

    public string DayStartTime { get; set; } = "08:00";
    public string DayEndTime { get; set; } = "17:00";
}