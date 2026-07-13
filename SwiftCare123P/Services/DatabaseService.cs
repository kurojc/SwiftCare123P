using SwiftCare123P.Models;
using System.Diagnostics;

namespace SwiftCare123P.Services;

public class DatabaseService : IDatabaseService
{
    private static bool _initialized = false;

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await Task.Delay(100);
        _initialized = true;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            await Task.Delay(100);

            var mockUsers = new List<User>
            {
                new()
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    Password = "password123",
                    Role = "CareSeeker",
                    ContactNumber = "09123456789",
                    Address = "123 Main St, Makati"
                },
                new()
                {
                    Id = 2,
                    FirstName = "Maria",
                    LastName = "Santos",
                    Email = "maria@example.com",
                    Password = "password123",
                    Role = "Caregiver",
                    ContactNumber = "09987654321",
                    Address = "456 Oak Ave, Quezon City"
                }
            };

            return mockUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetUserByEmailAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            await Task.Delay(100);
            var user = await GetUserByEmailAsync(email);
            return user != null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in EmailExistsAsync: {ex.Message}");
            return false;
        }
    }

    public async Task SaveUserAsync(User user)
    {
        try
        {
            await Task.Delay(100);
            Debug.WriteLine($"User saved: {user.Email}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SaveUserAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<CaregiverModel>> GetCaregiversAsync(string searchName = "", string searchService = "", int userId = 0)
    {
        var caregivers = new List<CaregiverModel>();

        try
        {
            await Task.Delay(500);

            caregivers = GetMockCaregivers();

            if (!string.IsNullOrEmpty(searchName))
            {
                caregivers = caregivers.Where(c => c.FullName?.Contains(searchName, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (!string.IsNullOrEmpty(searchService))
            {
                caregivers = caregivers.Where(c => c.ServicesOffered?.Contains(searchService, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetCaregiversAsync: {ex.Message}");
        }

        return caregivers;
    }

    public async Task<List<BookingModel>> GetUserBookingsAsync(int userId)
    {
        var bookings = new List<BookingModel>();

        try
        {
            await Task.Delay(500);

            bookings = new List<BookingModel>
            {
                new()
                {
                    BookingID = 1,
                    CaregiverID = 1,
                    CaregiverName = "Maria Santos",
                    ServiceName = "Child Care",
                    BookingDate = DateTime.Now.AddDays(2),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    Status = "Confirmed",
                    HasReview = false,
                    DateTimeDisplay = $"{DateTime.Now.AddDays(2):MMM dd, yyyy}\n09:00 – 17:00",
                    StatusColor = Color.FromArgb("#e0f7fa"),
                    StatusTextColor = Color.FromArgb("#006064"),
                    CanReview = false
                },
                new()
                {
                    BookingID = 2,
                    CaregiverID = 2,
                    CaregiverName = "Juan dela Cruz",
                    ServiceName = "Pet Care",
                    BookingDate = DateTime.Now.AddDays(-5),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    Status = "Completed",
                    HasReview = false,
                    DateTimeDisplay = $"{DateTime.Now.AddDays(-5):MMM dd, yyyy}\n10:00 – 16:00",
                    StatusColor = Color.FromArgb("#e8f5e9"),
                    StatusTextColor = Color.FromArgb("#2e7d32"),
                    CanReview = true
                }
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetUserBookingsAsync: {ex.Message}");
        }

        return bookings;
    }

    public async Task<UserModel?> GetUserProfileAsync(int userId)
    {
        UserModel? user = null;

        try
        {
            await Task.Delay(300);

            user = new UserModel
            {
                UserID = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                ContactNo = "09123456789",
                Gender = "Male",
                Birthdate = new DateTime(1990, 5, 15),
                Address = "123 Main St, Makati City"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetUserProfileAsync: {ex.Message}");
        }

        return user;
    }

    public async Task<bool> UpdateUserProfileAsync(UserModel user)
    {
        try
        {
            await Task.Delay(300);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in UpdateUserProfileAsync: {ex.Message}");
            return false;
        }
    }

    // ---------------------------------------------------------------
    // Caregiver dashboard support (mock data, same style as above)
    // ---------------------------------------------------------------

    public async Task<CaregiverModel?> GetCaregiverProfileAsync(int caregiverId)
    {
        try
        {
            await Task.Delay(300);

            var caregivers = GetMockCaregivers();
            return caregivers.FirstOrDefault(c => c.CaregiverID == caregiverId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetCaregiverProfileAsync: {ex.Message}");
            return null;
        }
    }

    public async Task UpdateCaregiverProfileAsync(CaregiverModel profile)
    {
        try
        {
            await Task.Delay(300);
            Debug.WriteLine($"Caregiver profile saved: {profile.CaregiverID} - {profile.FullName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in UpdateCaregiverProfileAsync: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateCaregiverAvailabilityAsync(int caregiverId, string selectedDays, TimeSpan startTime, TimeSpan endTime)
    {
        try
        {
            await Task.Delay(300);
            Debug.WriteLine($"Availability saved for caregiver {caregiverId}: days=[{selectedDays}] {startTime}-{endTime}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in UpdateCaregiverAvailabilityAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<BookingModel>> GetCaregiverBookingsAsync(int caregiverId)
    {
        var bookings = new List<BookingModel>();

        try
        {
            await Task.Delay(500);

            bookings = new List<BookingModel>
            {
                new()
                {
                    BookingID = 101,
                    CaregiverID = caregiverId,
                    CaregiverName = "Maria Santos",
                    ServiceName = "Child Care",
                    BookingDate = DateTime.Now.AddDays(1),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(13, 0, 0),
                    Status = "Pending",
                    HasReview = false,
                    DateTimeDisplay = $"{DateTime.Now.AddDays(1):MMM dd, yyyy}\n09:00 – 13:00",
                    StatusColor = Color.FromArgb("#fff8e1"),
                    StatusTextColor = Color.FromArgb("#8d6e00"),
                    CanReview = false
                },
                new()
                {
                    BookingID = 102,
                    CaregiverID = caregiverId,
                    CaregiverName = "Maria Santos",
                    ServiceName = "Elderly Care",
                    BookingDate = DateTime.Now.AddDays(3),
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    Status = "Confirmed",
                    HasReview = false,
                    DateTimeDisplay = $"{DateTime.Now.AddDays(3):MMM dd, yyyy}\n08:00 – 16:00",
                    StatusColor = Color.FromArgb("#e0f7fa"),
                    StatusTextColor = Color.FromArgb("#006064"),
                    CanReview = false
                },
                new()
                {
                    BookingID = 103,
                    CaregiverID = caregiverId,
                    CaregiverName = "Maria Santos",
                    ServiceName = "Child Care",
                    BookingDate = DateTime.Now.AddDays(-4),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(15, 0, 0),
                    Status = "Completed",
                    HasReview = true,
                    DateTimeDisplay = $"{DateTime.Now.AddDays(-4):MMM dd, yyyy}\n10:00 – 15:00",
                    StatusColor = Color.FromArgb("#e8f5e9"),
                    StatusTextColor = Color.FromArgb("#2e7d32"),
                    CanReview = false
                },
                new()
                {
                    BookingID = 104,
                    CaregiverID = caregiverId,
                    CaregiverName = "Maria Santos",
                    ServiceName = "Elderly Care",
                    BookingDate = DateTime.Now.AddDays(-10),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    Status = "Cancelled",
                    HasReview = false,
                    DateTimeDisplay = $"{DateTime.Now.AddDays(-10):MMM dd, yyyy}\n09:00 – 12:00",
                    StatusColor = Color.FromArgb("#fdecea"),
                    StatusTextColor = Color.FromArgb("#b71c1c"),
                    CanReview = false
                }
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetCaregiverBookingsAsync: {ex.Message}");
        }

        return bookings;
    }

    public async Task UpdateBookingStatusAsync(int bookingId, string status)
    {
        try
        {
            await Task.Delay(300);
            Debug.WriteLine($"Booking {bookingId} status updated to: {status}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in UpdateBookingStatusAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ReviewModel>> GetCaregiverReviewsAsync(int caregiverId)
    {
        var reviews = new List<ReviewModel>();

        try
        {
            await Task.Delay(400);

            reviews = new List<ReviewModel>
            {
                new()
                {
                    ReviewID = 1,
                    BookingID = 103,
                    UserID = 1,
                    CaregiverID = caregiverId,
                    Rating = 5,
                    ReviewText = "Very attentive and reliable. Would book again!",
                    ClientName = "John Doe",
                    ServiceName = "Child Care",
                    ReviewDate = DateTime.Now.AddDays(-3).ToString("MMM dd, yyyy"),
                    Stars = "★★★★★"
                },
                new()
                {
                    ReviewID = 2,
                    BookingID = 90,
                    UserID = 3,
                    CaregiverID = caregiverId,
                    Rating = 4,
                    ReviewText = "Great communication, showed up on time.",
                    ClientName = "Ana Reyes",
                    ServiceName = "Elderly Care",
                    ReviewDate = DateTime.Now.AddDays(-20).ToString("MMM dd, yyyy"),
                    Stars = "★★★★☆"
                }
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetCaregiverReviewsAsync: {ex.Message}");
        }

        return reviews;
    }

    // Shared mock caregiver dataset used by both GetCaregiversAsync and GetCaregiverProfileAsync
    private static List<CaregiverModel> GetMockCaregivers()
    {
        return new List<CaregiverModel>
        {
            new()
            {
                CaregiverID = 1,
                FullName = "Maria Santos",
                Address = "Makati City",
                ContactNo = "09123456789",
                HourlyRate = 500,
                HourlyRateDisplay = "₱500.00/Hour",
                AvailabilityStatus = "Available",
                AvailableDays = "1,2,3,4,5",
                AvailabilityStartTime = new TimeSpan(8, 0, 0),
                AvailabilityEndTime = new TimeSpan(17, 0, 0),
                Bio = "Experienced caregiver with 10 years of experience in child care and elderly care services.",
                ShortBio = "Experienced caregiver with 10 years...",
                ServicesOffered = "Child Care, Elderly Care",
                AvgRating = 4.8
            },
            new()
            {
                CaregiverID = 2,
                FullName = "Juan dela Cruz",
                Address = "Quezon City",
                ContactNo = "09987654321",
                HourlyRate = 450,
                HourlyRateDisplay = "₱450.00/Hour",
                AvailabilityStatus = "Available",
                AvailableDays = "1,3,4,5,6",
                AvailabilityStartTime = new TimeSpan(9, 0, 0),
                AvailabilityEndTime = new TimeSpan(18, 0, 0),
                Bio = "Specialized in pet care and house sitting with excellent references.",
                ShortBio = "Specialized in pet care and house...",
                ServicesOffered = "Pet Care, House Sitting",
                AvgRating = 4.5
            }
        };
    }
}