using SwiftCare123P.Models;
namespace SwiftCare123P.Services;
public interface IDatabaseService
{
    Task InitializeAsync();
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task SaveUserAsync(User user);

    // --- CareSeeker dashboard ---
    Task<List<CaregiverModel>> GetCaregiversAsync(string searchName = "", string searchService = "", int userId = 0);
    Task<List<BookingModel>> GetUserBookingsAsync(int userId);
    Task<UserModel?> GetUserProfileAsync(int userId);
    Task<bool> UpdateUserProfileAsync(UserModel user);

    // --- Caregiver profile ---
    // NOTE: the "caregiverId" parameter here is actually the caregiver's UserID
    // (that's what the dashboards have on hand after login), not CaregiverProfiles.CaregiverID.
    Task<CaregiverModel?> GetCaregiverProfileAsync(int caregiverId);
    Task UpdateCaregiverProfileAsync(CaregiverModel profile);

    // --- Availability ---
    // selectedDays: comma-separated day indices, e.g. "0,2,4" (0=Monday ... 6=Sunday) — matches the web app's day order
    Task UpdateCaregiverAvailabilityAsync(int caregiverId, string selectedDays, TimeSpan startTime, TimeSpan endTime);

    // --- Bookings ---
    Task<List<BookingModel>> GetCaregiverBookingsAsync(int caregiverId);
    Task UpdateBookingStatusAsync(int bookingId, string status);

    // caregiverId is the caregiver's UserID (same convention as GetCaregiverProfileAsync above),
    // NOT CaregiverProfiles.CaregiverID — the implementation resolves that internally.
    // Returns false if no caregiver profile exists for that user.
    Task<bool> CreateBookingAsync(int userId, int caregiverId, int serviceId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime);

    // --- Services ---
    Task<List<ServiceModel>> GetServicesAsync();

    // --- Reviews ---
    Task<List<ReviewModel>> GetCaregiverReviewsAsync(int caregiverId);
}