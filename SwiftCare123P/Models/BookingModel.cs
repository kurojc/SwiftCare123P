namespace SwiftCare123P.Models;

public class BookingModel
{
    public int BookingID { get; set; }
    public int CaregiverID { get; set; }

    /// <summary>
    /// The "other party" in this booking: the caregiver's name when a care seeker is
    /// looking at their own bookings, or the client's name when a caregiver is looking
    /// at bookings made against them. Whichever view built this model has already
    /// resolved the correct name — see DatabaseService.ToBookingModelAsync.
    /// </summary>
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
    public bool CanCancel { get; set; }
    public string? DateTimeDisplay { get; set; }

    // --- Caregiver-side action availability (computed from Status, caregiver view only) ---
    public bool CanAccept { get; set; }
    public bool CanDecline { get; set; }
    public bool CanComplete { get; set; }

    public decimal TotalPrice { get; set; }
}