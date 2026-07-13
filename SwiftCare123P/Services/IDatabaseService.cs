using SwiftCare123P.Models;
namespace SwiftCare123P.Services;
public interface IDatabaseService
{
    Task InitializeAsync();
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task SaveUserAsync(User user);

    // --- Caregiver profile ---
    Task<CaregiverModel?> GetCaregiverProfileAsync(int caregiverId);
    Task UpdateCaregiverProfileAsync(CaregiverModel profile);

    // --- Availability ---
    // selectedDays: comma-separated day indices, e.g. "0,2,4" (0=Sunday ... 6=Saturday)
    Task UpdateCaregiverAvailabilityAsync(int caregiverId, string selectedDays, TimeSpan startTime, TimeSpan endTime);

    // --- Bookings ---
    Task<List<BookingModel>> GetCaregiverBookingsAsync(int caregiverId);
    Task UpdateBookingStatusAsync(int bookingId, string status);

    // --- Reviews ---
    Task<List<ReviewModel>> GetCaregiverReviewsAsync(int caregiverId);
}