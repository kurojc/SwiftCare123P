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
            // TODO: Implement database call to fetch caregiver profile
            // For now, using placeholder data
            ProfileModel = new CaregiverModel
            {
                FullName = GreetingName,
                ContactNo = "",
                Address = "",
                HourlyRateDisplay = "",
                Bio = ""
            };
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
            // TODO: Implement database calls for stats
            // Placeholder implementation
            TotalBookings = "0";
            PendingBookings = "0";
            CompletedBookings = "0";
            AverageRating = "�";
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
            HasNoBookings = true;
            HasNoAllBookings = true;

            // TODO: Implement database call to fetch bookings
            // var bookings = await _dbService.GetCaregiverBookingsAsync(_caregiverId);
            // foreach (var booking in bookings)
            // {
            //     AllBookings.Add(booking);
            //     if (RecentBookings.Count < 5)
            //         RecentBookings.Add(booking);
            // }
            // HasNoBookings = RecentBookings.Count == 0;
            // HasNoAllBookings = AllBookings.Count == 0;

            await Task.CompletedTask;
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
            HasNoReviews = true;
            ReviewCount = "0 reviews";

            // TODO: Implement database call to fetch reviews
            // var reviews = await _dbService.GetCaregiverReviewsAsync(_caregiverId);
            // foreach (var review in reviews)
            // {
            //     Reviews.Add(review);
            // }
            // HasNoReviews = Reviews.Count == 0;
            // ReviewCount = $"{Reviews.Count} review{(Reviews.Count > 1 ? "s" : "")}";

            await Task.CompletedTask;
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
            // TODO: Implement database call to fetch availability
            // For now, using default times
            DayStartTime = new TimeSpan(8, 0, 0);
            DayEndTime = new TimeSpan(17, 0, 0);

            await Task.CompletedTask;
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

            // TODO: Implement database save for profile
            // await _dbService.UpdateCaregiverProfileAsync(ProfileModel);

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

            // TODO: Implement database save for availability
            // await _dbService.UpdateCaregiverAvailabilityAsync(_caregiverId, selectedDays, DayStartTime, DayEndTime);

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

            // TODO: Implement database update for booking status
            // await _dbService.UpdateBookingStatusAsync(bookingId, "Confirmed");

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

            // TODO: Implement database update for booking status
            // await _dbService.UpdateBookingStatusAsync(bookingId, "Cancelled");

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

            // TODO: Implement database update for booking status
            // await _dbService.UpdateBookingStatusAsync(bookingId, "Completed");

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