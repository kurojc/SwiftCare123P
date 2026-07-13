using Microsoft.Maui.Controls;
using SwiftCare123P.Models;
using SwiftCare123P.Services;
using System.Collections.ObjectModel;

namespace SwiftCare123P;

public partial class UserDashboard : ContentPage
{
    private readonly IDatabaseService _dbService;
    private ObservableCollection<CaregiverModel> _caregivers;
    private ObservableCollection<BookingModel> _bookings;
    private string _currentFilter = "all";
    private int _userId;

    public UserDashboard()
    {
        InitializeComponent();
        _dbService = AppServices.Database;
        _caregivers = new ObservableCollection<CaregiverModel>();
        _bookings = new ObservableCollection<BookingModel>();
        rptCaregivers.ItemsSource = _caregivers;
        rptBookings.ItemsSource = _bookings;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            string? userIdStr = await SecureStorage.Default.GetAsync("UserID");
            string? userName = await SecureStorage.Default.GetAsync("UserName");

            if (!int.TryParse(userIdStr, out _userId) || _userId == 0)
            {
                _userId = 1;
            }

            lblGreetName.Text = userName ?? "User";
            lblProfileName.Text = userName ?? "User";

            await LoadInitialData();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
        }
    }

    private async Task LoadInitialData()
    {
        try
        {
            await LoadCaregivers();
            await LoadBookings();
            await LoadProfile();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }

    private async Task LoadCaregivers(string searchName = "", string searchService = "")
    {
        try
        {
            await Task.Delay(500);

            var caregivers = new List<CaregiverModel>
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

            _caregivers.Clear();
            foreach (var caregiver in caregivers)
            {
                _caregivers.Add(caregiver);
            }

            pnlNoCaregivers.IsVisible = _caregivers.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load caregivers: {ex.Message}", "OK");
        }
    }

    private async Task LoadBookings()
    {
        try
        {
            await Task.Delay(500);

            var bookings = new List<BookingModel>
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

            _bookings.Clear();
            foreach (var booking in bookings)
            {
                _bookings.Add(booking);
            }

            pnlNoBookings.IsVisible = _bookings.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load bookings: {ex.Message}", "OK");
        }
    }

    private async Task LoadProfile()
    {
        try
        {
            await Task.Delay(300);

            txtProfileFirstName.Text = "John";
            txtProfileLastName.Text = "Doe";
            txtProfileEmail.Text = "john.doe@example.com";
            txtProfileContact.Text = "09123456789";
            txtProfileGender.Text = "Male";
            txtProfileBirthdate.Text = "May 15, 1990";
            txtProfileAddress.Text = "123 Main St, Makati City";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
        }
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string searchName = txtSearch.Text?.Trim() ?? "";
        string searchService = ddlService.SelectedIndex > 0 ? (string?)ddlService.SelectedItem ?? "" : "";

        await LoadCaregivers(searchName, searchService);
    }

    private void OnContactCaregiverClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CaregiverModel caregiver)
        {
            DisplayAlert("Contact", $"Contact {caregiver.FullName} at {caregiver.ContactNo}", "OK");
        }
    }

    private void OnFilterBookingsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string status)
        {
            _currentFilter = status;
            ApplyBookingsFilter();
        }
    }

    private void ApplyBookingsFilter()
    {
        var filteredBookings = _currentFilter switch
        {
            "pending" => _bookings.Where(b => b.Status == "Pending"),
            "confirmed" => _bookings.Where(b => b.Status == "Confirmed"),
            "completed" => _bookings.Where(b => b.Status == "Completed"),
            "cancelled" => _bookings.Where(b => b.Status == "Cancelled"),
            _ => _bookings
        };

        rptBookings.ItemsSource = new ObservableCollection<BookingModel>(filteredBookings);
    }

    private async void OnLeaveReviewClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is BookingModel booking)
        {
            await DisplayAlert("Review", $"Review for booking {booking.BookingID}", "OK");
        }
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        try
        {
            lblProfileMsg.Text = "✓ Profile updated successfully!";
            lblProfileMsg.TextColor = Color.FromArgb("#00838f");
        }
        catch (Exception ex)
        {
            lblProfileMsg.Text = $"Error: {ex.Message}";
            lblProfileMsg.TextColor = Color.FromArgb("#e53935");
        }
    }

    private void OnTabBrowseClicked(object sender, EventArgs e) => ShowPanel("browse");
    private void OnTabBookingsClicked(object sender, EventArgs e) => ShowPanel("bookings");
    private void OnTabProfileClicked(object sender, EventArgs e) => ShowPanel("profile");

    private void ShowPanel(string panelName)
    {
        PanelBrowse.IsVisible = panelName == "browse";
        PanelBookings.IsVisible = panelName == "bookings";
        PanelProfile.IsVisible = panelName == "profile";

        TabBrowse.TextColor = panelName == "browse" ? Color.FromArgb("#00838f") : Color.FromArgb("#4a7a92");
        TabBookings.TextColor = panelName == "bookings" ? Color.FromArgb("#00838f") : Color.FromArgb("#4a7a92");
        TabProfile.TextColor = panelName == "profile" ? Color.FromArgb("#00838f") : Color.FromArgb("#4a7a92");

        TopbarTitle.Text = panelName switch
        {
            "browse" => "Browse Caregivers",
            "bookings" => "My Bookings",
            "profile" => "My Profile",
            _ => "SwiftCare"
        };
    }
}