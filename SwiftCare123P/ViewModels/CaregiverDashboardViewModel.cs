using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwiftCare123P.Models;
using SwiftCare123P.Services;

namespace SwiftCare123P.ViewModels;

public partial class CaregiverDashboardViewModel : ObservableObject
{
    private readonly IDatabaseService _dbService;
    private int _caregiverId;

    [ObservableProperty]
    private string topbarTitle = "Overview";

    [ObservableProperty]
    private string greetingName = "Caregiver";

    [ObservableProperty]
    private string profileName = "Your Name";

    [ObservableProperty]
    private string totalBookings = "0";

    [ObservableProperty]
    private string pendingBookings = "0";

    [ObservableProperty]
    private string completedBookings = "0";

    [ObservableProperty]
    private string averageRating = "�";

    [ObservableProperty]
    private bool isOverviewPanelVisible = true;

    [ObservableProperty]
    private bool isProfilePanelVisible = false;

    [ObservableProperty]
    private bool isBookingsPanelVisible = false;

    [ObservableProperty]
    private bool isAvailabilityPanelVisible = false;

    [ObservableProperty]
    private bool isReviewsPanelVisible = false;

    [ObservableProperty]
    private bool hasNoBookings = true;

    [ObservableProperty]
    private bool hasNoAllBookings = true;

    [ObservableProperty]
    private bool hasNoReviews = true;

    [ObservableProperty]
    private string bioCharCount = "(0/150)";

    [ObservableProperty]
    private string reviewCount = "0 reviews";

    [ObservableProperty]
    private TimeSpan dayStartTime = new TimeSpan(8, 0, 0);

    [ObservableProperty]
    private TimeSpan dayEndTime = new TimeSpan(17, 0, 0);

    [ObservableProperty]
    private CaregiverModel profileModel = new();

    public ObservableCollection<BookingModel> RecentBookings { get; } = [];
    public ObservableCollection<BookingModel> AllBookings { get; } = [];
    public ObservableCollection<ReviewModel> Reviews { get; } = [];
    public ObservableCollection<bool> AvailableDays { get; } = new([false, false, false, false, false, false, false]);
    public ObservableCollection<bool> IsServiceSelected { get; } = new([false, false, false, false, false]);

    public CaregiverDashboardViewModel()
    {
        _dbService = AppServices.Database;
    }

    [RelayCommand]
    public async Task LoadData()
    {
        try
        {
            string? caregiverIdStr = await SecureStorage.Default.GetAsync("UserID");
            string? userName = await SecureStorage.Default.GetAsync("UserName");

            if (!int.TryParse(caregiverIdStr, out _caregiverId) || _caregiverId == 0)
            {
                _caregiverId = 1;
            }

            GreetingName = userName ?? "Caregiver";
            ProfileName = userName ?? "Your Name";

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await LoadProfile();
                await LoadStats();
                await LoadBookings();
                await LoadReviews();
                await LoadAvailability();
            });
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Failed to initialize: {ex.Message}");
        }
    }

    private async Task LoadProfile()
    {
        try
        {
            var profile = await _dbService.GetCaregiverProfileAsync(_caregiverId);

            if (profile != null)
            {
                ProfileModel = profile;
                ProfileName = string.IsNullOrWhiteSpace(profile.FullName) ? GreetingName : profile.FullName;
                BioCharCount = $"({profile.Bio?.Length ?? 0}/150)";
            }
            else
            {
                // No profile on record yet - fall back to placeholder defaults
                ProfileModel = new CaregiverModel
                {
                    CaregiverID = _caregiverId,
                    FullName = GreetingName,
                    ContactNo = "",
                    Address = "",
                    HourlyRateDisplay = "",
                    Bio = ""
                };
                BioCharCount = "(0/150)";
            }
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Failed to load profile: {ex.Message}");
        }
    }

    private async Task LoadStats()
    {
        try
        {
            var bookings = await _dbService.GetCaregiverBookingsAsync(_caregiverId);

            TotalBookings = bookings.Count.ToString();
            PendingBookings = bookings.Count(b => b.Status == "Pending").ToString();
            CompletedBookings = bookings.Count(b => b.Status == "Completed").ToString();

            var reviews = await _dbService.GetCaregiverReviewsAsync(_caregiverId);
            AverageRating = reviews.Count > 0
                ? reviews.Average(r => r.Rating).ToString("0.0")
                : (ProfileModel.AvgRating > 0 ? ProfileModel.AvgRating.ToString("0.0") : "N/A");
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Failed to load stats: {ex.Message}");
        }
    }

    private async Task LoadBookings()
    {
        try
        {
            RecentBookings.Clear();
            AllBookings.Clear();

            var bookings = await _dbService.GetCaregiverBookingsAsync(_caregiverId);

            foreach (var booking in bookings.OrderByDescending(b => b.BookingDate))
            {
                AllBookings.Add(booking);
                if (RecentBookings.Count < 5)
                    RecentBookings.Add(booking);
            }

            HasNoBookings = RecentBookings.Count == 0;
            HasNoAllBookings = AllBookings.Count == 0;
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Failed to load bookings: {ex.Message}");
        }
    }

    private async Task LoadReviews()
    {
        try
        {
            Reviews.Clear();

            var reviews = await _dbService.GetCaregiverReviewsAsync(_caregiverId);

            foreach (var review in reviews)
            {
                Reviews.Add(review);
            }

            HasNoReviews = Reviews.Count == 0;
            ReviewCount = $"{Reviews.Count} review{(Reviews.Count == 1 ? "" : "s")}";
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Failed to load reviews: {ex.Message}");
        }
    }

    private async Task LoadAvailability()
    {
        try
        {
            // Availability is stored directly on CaregiverModel:
            // - AvailableDays: comma-separated day indices, e.g. "0,2,4" (0=Sunday ... 6=Saturday)
            // - AvailabilityStartTime / AvailabilityEndTime: daily working window
            var profile = await _dbService.GetCaregiverProfileAsync(_caregiverId);

            if (profile != null)
            {
                DayStartTime = profile.AvailabilityStartTime;
                DayEndTime = profile.AvailabilityEndTime;

                var selectedIndices = (profile.AvailableDays ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToHashSet();

                for (int i = 0; i < AvailableDays.Count; i++)
                {
                    AvailableDays[i] = selectedIndices.Contains(i);
                }
            }
            else
            {
                // No profile on record yet - use sensible defaults
                DayStartTime = new TimeSpan(8, 0, 0);
                DayEndTime = new TimeSpan(17, 0, 0);

                for (int i = 0; i < AvailableDays.Count; i++)
                {
                    AvailableDays[i] = false;
                }
            }
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Failed to load availability: {ex.Message}");
        }
    }

    [RelayCommand]
    public void ShowPanel(string panelName)
    {
        IsOverviewPanelVisible = panelName == "overview";
        IsProfilePanelVisible = panelName == "profile";
        IsBookingsPanelVisible = panelName == "bookings";
        IsAvailabilityPanelVisible = panelName == "availability";
        IsReviewsPanelVisible = panelName == "reviews";

        TopbarTitle = panelName switch
        {
            "overview" => "Overview",
            "profile" => "My Profile",
            "bookings" => "Bookings",
            "availability" => "Availability",
            "reviews" => "Reviews",
            _ => "Overview"
        };
    }

    [RelayCommand]
    public async Task SaveProfile()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ProfileModel?.FullName))
            {
                await ShowAlert("Validation", "Please enter your full name");
                return;
            }

            await _dbService.UpdateCaregiverProfileAsync(ProfileModel);

            ProfileName = ProfileModel.FullName;

            await ShowAlert("Success", "? Profile saved successfully!");
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task SaveAvailability()
    {
        try
        {
            var selectedDays = string.Join(",", AvailableDays
                .Select((selected, index) => selected ? index.ToString() : null)
                .Where(x => x != null));

            await _dbService.UpdateCaregiverAvailabilityAsync(_caregiverId, selectedDays, DayStartTime, DayEndTime);

            // Keep the in-memory profile consistent with what was just saved
            ProfileModel.AvailableDays = selectedDays;
            ProfileModel.AvailabilityStartTime = DayStartTime;
            ProfileModel.AvailabilityEndTime = DayEndTime;

            await ShowAlert("Success", "? Availability saved!");
            await LoadAvailability();
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task AcceptBooking(object? parameter)
    {
        try
        {
            if (!int.TryParse(parameter?.ToString(), out int bookingId))
                return;

            await _dbService.UpdateBookingStatusAsync(bookingId, "Confirmed");

            await ShowAlert("Success", "? Booking accepted!");
            await LoadBookings();
            await LoadStats();
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task DeclineBooking(object? parameter)
    {
        try
        {
            if (!int.TryParse(parameter?.ToString(), out int bookingId))
                return;

            await _dbService.UpdateBookingStatusAsync(bookingId, "Cancelled");

            await ShowAlert("Success", "Booking declined.");
            await LoadBookings();
            await LoadStats();
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task CompleteBooking(object? parameter)
    {
        try
        {
            if (!int.TryParse(parameter?.ToString(), out int bookingId))
                return;

            bool confirmed = await ShowConfirmation(
                "Mark as Completed",
                "This action cannot be undone");

            if (!confirmed)
                return;

            await _dbService.UpdateBookingStatusAsync(bookingId, "Completed");

            await ShowAlert("Success", "? Booking marked as completed!");
            await LoadBookings();
            await LoadStats();
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Error: {ex.Message}");
        }
    }

    public void OnBioTextChanged(string text)
    {
        BioCharCount = $"({text?.Length ?? 0}/150)";
    }

    private async Task ShowAlert(string title, string message)
    {
        try
        {
            if (Application.Current?.Windows.Count > 0)
            {
                var page = Application.Current.Windows[0].Page;
                if (page != null)
                {
                    await page.DisplayAlert(title, message, "OK");
                }
            }
        }
        catch
        {
            // Fallback if page is not accessible
        }
    }

    private async Task<bool> ShowConfirmation(string title, string message)
    {
        try
        {
            if (Application.Current?.Windows.Count > 0)
            {
                var page = Application.Current.Windows[0].Page;
                if (page != null)
                {
                    return await page.DisplayAlert(title, message, "Confirm", "Cancel");
                }
            }
        }
        catch
        {
            // Fallback if page is not accessible
        }

        return false;
    }
}