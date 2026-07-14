using Microsoft.Maui.Controls;
using SwiftCare123P.Common;
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
        NavigationPage.SetHasNavigationBar(this, false);
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
            var caregivers = await _dbService.GetCaregiversAsync(searchName, searchService, _userId);

            _caregivers.Clear();
            foreach (var caregiver in caregivers)
                _caregivers.Add(caregiver);

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
            var bookings = await _dbService.GetUserBookingsAsync(_userId);

            _bookings.Clear();
            foreach (var booking in bookings)
                _bookings.Add(booking);

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
            var profile = await _dbService.GetUserProfileAsync(_userId);
            if (profile is null) return;

            txtProfileFirstName.Text = profile.FirstName;
            txtProfileLastName.Text = profile.LastName;
            txtProfileEmail.Text = profile.Email;
            txtProfileContact.Text = profile.ContactNo;
            txtProfileGender.Text = profile.Gender;
            txtProfileBirthdate.Text = profile.Birthdate?.ToString("MMMM dd, yyyy") ?? string.Empty;
            txtProfileAddress.Text = profile.Address;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
        }
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        try
        {
            var updated = new UserModel
            {
                UserID = _userId,
                FirstName = txtProfileFirstName.Text?.Trim(),
                LastName = txtProfileLastName.Text?.Trim(),
                ContactNo = txtProfileContact.Text?.Trim(),
                Gender = txtProfileGender.Text?.Trim(),
                Address = txtProfileAddress.Text?.Trim()
            };

            var success = await _dbService.UpdateUserProfileAsync(updated);

            lblProfileMsg.Text = success ? "✓ Profile updated successfully!" : "Could not find your account.";
            lblProfileMsg.TextColor = success ? Color.FromArgb("#00838f") : Color.FromArgb("#e53935");
        }
        catch (Exception ex)
        {
            lblProfileMsg.Text = $"Error: {ex.Message}";
            lblProfileMsg.TextColor = Color.FromArgb("#e53935");
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

    private async void OnBookNowClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CaregiverModel caregiver)
        {
            await Navigation.PushAsync(new BookingPage(caregiver));
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
            "pending" => _bookings.Where(b => b.Status == BookingStatus.Pending),
            "confirmed" => _bookings.Where(b => b.Status == BookingStatus.Confirmed),
            "completed" => _bookings.Where(b => b.Status == BookingStatus.Completed),
            "cancelled" => _bookings.Where(b => b.Status == BookingStatus.Cancelled),
            _ => _bookings
        };

        rptBookings.ItemsSource = new ObservableCollection<BookingModel>(filteredBookings);
    }

    private async void OnLeaveReviewClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not BookingModel booking)
            return;

        var ratingChoice = await DisplayActionSheet("Rate your experience", "Cancel", null,
            "⭐ 1", "⭐⭐ 2", "⭐⭐⭐ 3", "⭐⭐⭐⭐ 4", "⭐⭐⭐⭐⭐ 5");

        if (string.IsNullOrEmpty(ratingChoice) || ratingChoice == "Cancel")
            return;

        int rating = int.Parse(ratingChoice.Split(' ')[^1]);

        var comment = await DisplayPromptAsync(
            "Leave a Review",
            "Tell us about your experience (optional):",
            accept: "Submit",
            cancel: "Skip",
            maxLength: 200) ?? string.Empty;

        var success = await _dbService.CreateReviewAsync(booking.BookingID, _userId, rating, comment);

        if (success)
        {
            await DisplayAlert("Thank You!", "Your review has been submitted.", "OK");
            await LoadBookings(); // refresh so this booking now shows "✓ Reviewed"
        }
        else
        {
            await DisplayAlert("Error", "Could not submit your review. It may have already been reviewed.", "OK");
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

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert(
            "Logout",
            "Do you want to logout?",
            "Yes",
            "Cancel");

        if (!answer)
            return;

        SecureStorage.Default.RemoveAll();

        Application.Current!.MainPage =
            new NavigationPage(new MainPage());
    }

}