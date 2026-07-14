using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwiftCare123P.Common;
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
    private string averageRating = "—";

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

    // Fixed-length 7-slot toggle grid for the Availability panel, in DayOfWeekHelper's Mon=0..Sun=6 order.
    [ObservableProperty] private bool isMondaySelected;
    [ObservableProperty] private bool isTuesdaySelected;
    [ObservableProperty] private bool isWednesdaySelected;
    [ObservableProperty] private bool isThursdaySelected;
    [ObservableProperty] private bool isFridaySelected;
    [ObservableProperty] private bool isSaturdaySelected;
    [ObservableProperty] private bool isSundaySelected;

    // Loaded from the database rather than hardcoded, so it always matches whatever
    // services actually exist there instead of a hand-typed, easily-out-of-sync list.
    public ObservableCollection<ServiceSelectionItem> AvailableServices { get; } = [];

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
                await LoadServices();
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

    [RelayCommand]
    public void Logout()
    {
        SecureStorage.Default.RemoveAll();

        Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());

    }

    private async Task LoadServices()
    {
        try
        {
            var services = await _dbService.GetServicesAsync();

            AvailableServices.Clear();
            foreach (var service in services)
                AvailableServices.Add(new ServiceSelectionItem { ServiceID = service.ServiceID, ServiceName = service.ServiceName });
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Failed to load services: {ex.Message}");
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

                var selected = (profile.ServicesOffered ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var service in AvailableServices)
                    service.IsSelected = selected.Contains(service.ServiceName, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                
                var user = await _dbService.GetUserProfileAsync(_caregiverId);

                ProfileModel = new CaregiverModel
                {
                    CaregiverID = _caregiverId,
                    FullName = user is not null ? $"{user.FirstName} {user.LastName}".Trim() : GreetingName,
                    ContactNo = user?.ContactNo ?? "",
                    Address = user?.Address ?? "",
                    HourlyRateDisplay = "",
                    Bio = ""
                };
                BioCharCount = "(0/150)";

                foreach (var service in AvailableServices)
                    service.IsSelected = false;
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
            PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending).ToString();
            CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed).ToString();

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
            var profile = await _dbService.GetCaregiverProfileAsync(_caregiverId);

            if (profile != null)
            {
                DayStartTime = profile.AvailabilityStartTime;
                DayEndTime = profile.AvailabilityEndTime;

                var selectedIndices = DayOfWeekHelper.ParseAvailableDays(profile.AvailableDays);

                IsMondaySelected = selectedIndices.Contains(0);
                IsTuesdaySelected = selectedIndices.Contains(1);
                IsWednesdaySelected = selectedIndices.Contains(2);
                IsThursdaySelected = selectedIndices.Contains(3);
                IsFridaySelected = selectedIndices.Contains(4);
                IsSaturdaySelected = selectedIndices.Contains(5);
                IsSundaySelected = selectedIndices.Contains(6);
            }
            else
            {
                DayStartTime = new TimeSpan(8, 0, 0);
                DayEndTime = new TimeSpan(17, 0, 0);

                IsMondaySelected = IsTuesdaySelected = IsWednesdaySelected = IsThursdaySelected =
                    IsFridaySelected = IsSaturdaySelected = IsSundaySelected = false;
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

            ProfileModel.ServicesOffered = string.Join(", ", AvailableServices
                .Where(s => s.IsSelected)
                .Select(s => s.ServiceName));

            await _dbService.UpdateCaregiverProfileAsync(ProfileModel);

            ProfileName = ProfileModel.FullName;

            await ShowAlert("Success", "✓ Profile saved successfully!");
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
            var selectedDays = DayOfWeekHelper.ToCsv(new[]{
            IsMondaySelected, IsTuesdaySelected, IsWednesdaySelected, IsThursdaySelected,
            IsFridaySelected, IsSaturdaySelected, IsSundaySelected});

            await _dbService.UpdateCaregiverAvailabilityAsync(_caregiverId, selectedDays, DayStartTime, DayEndTime);

            // Keep the in-memory profile consistent with what was just saved
            ProfileModel.AvailableDays = selectedDays;
            ProfileModel.AvailabilityStartTime = DayStartTime;
            ProfileModel.AvailabilityEndTime = DayEndTime;

            await ShowAlert("Success", "✓ Availability saved!");
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

            await _dbService.UpdateBookingStatusAsync(bookingId, BookingStatus.Confirmed);

            await ShowAlert("Success", "✓ Booking accepted!");
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

            await _dbService.UpdateBookingStatusAsync(bookingId, BookingStatus.Cancelled);

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

            await _dbService.UpdateBookingStatusAsync(bookingId, BookingStatus.Completed);

            await ShowAlert("Success", "✓ Booking marked as completed!");
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