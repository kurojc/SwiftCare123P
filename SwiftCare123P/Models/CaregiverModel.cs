namespace SwiftCare123P.Models;
public class CaregiverModel
{
    public int CaregiverID { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? ContactNo { get; set; }
    public decimal HourlyRate { get; set; }
    public string? HourlyRateDisplay { get; set; }
    public string? AvailabilityStatus { get; set; }
    public string? AvailableDays { get; set; }

    public TimeSpan AvailabilityStartTime { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan AvailabilityEndTime { get; set; } = new TimeSpan(17, 0, 0);

    public string? Bio { get; set; }
    public string? ShortBio { get; set; }
    public string? ServicesOffered { get; set; }
    public double AvgRating { get; set; }
}