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

            caregivers = new List<CaregiverModel>
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
                    Bio = "Specialized in pet care and house sitting with excellent references.",
                    ShortBio = "Specialized in pet care and house...",
                    ServicesOffered = "Pet Care, House Sitting",
                    AvgRating = 4.5
                }
            };

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
}
