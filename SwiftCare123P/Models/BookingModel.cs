namespace SwiftCare123P.Models;

public class BookingModel
{
    public int BookingID { get; set; }
    public int CaregiverID { get; set; }
    public string? CaregiverName { get; set; }
    public string? ServiceName { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Status { get; set; }
    public bool HasReview { get; set; }
    public Color? StatusColor { get; set; }
    public Color? StatusTextColor { get; set; }
    public bool CanReview { get; set; }
    public string? DateTimeDisplay { get; set; }
}