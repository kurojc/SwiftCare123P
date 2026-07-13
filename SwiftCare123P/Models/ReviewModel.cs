namespace SwiftCare123P.Models;

public class ReviewModel
{
    public int ReviewID { get; set; }
    public int BookingID { get; set; }
    public int UserID { get; set; }
    public int CaregiverID { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public string? ClientName { get; set; }
    public string? ServiceName { get; set; }
    public string? ReviewDate { get; set; }
    public string? Stars { get; set; }
}