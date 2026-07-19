using SwiftCare123P.Common;
using SwiftCare123P.Models;
using SwiftCare123P.Services;

namespace SwiftCare123P;

public partial class BookingPage : ContentPage
{
    private readonly IDatabaseService _dbService;
    private readonly CaregiverModel _caregiver;
    private int _userId;
    private List<ServiceModel> _services = new();

    // Caregiver's declared weekly availability, parsed once via the shared helper
    // so this page's calendar can never disagree with how the caregiver's own
    // Availability screen interprets the same stored string.
    private readonly HashSet<int> _availableWeekdays;

    private DateTime _calendarMonthAnchor = DateTime.Today;
    private DateTime? _selectedDate;
    private const decimal BookingFee = 30m;

    public BookingPage(CaregiverModel caregiver)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _dbService = AppServices.Database;
        _caregiver = caregiver;
        _availableWeekdays = DayOfWeekHelper.ParseAvailableDays(caregiver.AvailableDays);

        lblCaregiverName.Text = caregiver.FullName;
        lblCaregiverRate.Text = string.IsNullOrWhiteSpace(caregiver.HourlyRateDisplay)
            ? $"₱{caregiver.HourlyRate:0.00}/Hour"
            : caregiver.HourlyRateDisplay;
        lblCaregiverRating.Text = caregiver.AvgRating > 0
            ? $"⭐ {caregiver.AvgRating:0.0} rating"
            : "⭐ New caregiver";

        tpStartTime.Time = caregiver.AvailabilityStartTime;
        tpEndTime.Time = caregiver.AvailabilityEndTime;

        bool hasAvailability = _availableWeekdays.Count > 0;
        lblNoAvailability.IsVisible = !hasAvailability;
        gridCalendarHeader.IsVisible = hasAvailability;
        gridCalendarNav.IsVisible = hasAvailability;

        lblAvailabilityHint.Text = hasAvailability
            ? $"Available {DayOfWeekHelper.Describe(_availableWeekdays)} · {caregiver.AvailabilityStartTime:hh\\:mm} – {caregiver.AvailabilityEndTime:hh\\:mm}"
            : string.Empty;

        pckService.SelectedIndexChanged += (_, _) => UpdateSummary();
        tpStartTime.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(TimePicker.Time)) UpdateSummary(); };
        tpEndTime.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(TimePicker.Time)) UpdateSummary(); };

        if (hasAvailability)
            BuildCalendar(_calendarMonthAnchor);

        UpdateSummary();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            string? userIdStr = await SecureStorage.Default.GetAsync("UserID");
            if (!int.TryParse(userIdStr, out _userId) || _userId == 0)
            {
                _userId = 1;
            }

            await LoadServices();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
        }
    }

    private async Task LoadServices()
    {
        try
        {
            _services = await _dbService.GetServicesAsync();
            pckService.ItemsSource = _services;

            // Pre-select a service this caregiver actually lists, if we can match one
            if (!string.IsNullOrWhiteSpace(_caregiver.ServicesOffered))
            {
                var offered = _caregiver.ServicesOffered
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var match = _services.FirstOrDefault(s =>
                    offered.Contains(s.ServiceName, StringComparer.OrdinalIgnoreCase));

                if (match is not null)
                    pckService.SelectedItem = match;
            }

            UpdateSummary();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load services: {ex.Message}", "OK");
        }
    }

    // ───────────────────────── Calendar ─────────────────────────

    private void BuildCalendar(DateTime monthAnchor)
    {
        gridCalendarDays.Children.Clear();
        gridCalendarDays.RowDefinitions.Clear();

        var firstOfMonth = new DateTime(monthAnchor.Year, monthAnchor.Month, 1);
        lblCalendarMonth.Text = firstOfMonth.ToString("MMMM yyyy");

        int leadingBlanks = DayOfWeekHelper.ToAppIndex(firstOfMonth.DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(firstOfMonth.Year, firstOfMonth.Month);
        int totalCells = leadingBlanks + daysInMonth;
        int rows = (int)Math.Ceiling(totalCells / 7.0);

        for (int r = 0; r < rows; r++)
            gridCalendarDays.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var firstSelectableMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        btnPrevMonth.IsEnabled = firstOfMonth > firstSelectableMonth;

        for (int cell = 0; cell < totalCells; cell++)
        {
            int day = cell - leadingBlanks + 1;
            if (day < 1) continue;

            var date = new DateTime(firstOfMonth.Year, firstOfMonth.Month, day);
            int weekdayIndex = DayOfWeekHelper.ToAppIndex(date.DayOfWeek);
            bool isPast = date.Date < DateTime.Today;
            bool isAvailableWeekday = _availableWeekdays.Contains(weekdayIndex);
            bool isSelectable = isAvailableWeekday && !isPast;
            bool isSelected = _selectedDate.HasValue && _selectedDate.Value.Date == date.Date;

            var cellButton = new Button
            {
                Text = day.ToString(),
                FontSize = 13,
                CornerRadius = 8,
                Padding = new Thickness(0),
                HeightRequest = 40,
                IsEnabled = isSelectable,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#006064")
                    : isSelectable ? Color.FromArgb("#e0f7fa") : Color.FromArgb("#f2f2f2"),
                TextColor = isSelected
                    ? Colors.White
                    : isSelectable ? Color.FromArgb("#006064") : Color.FromArgb("#b0b8bd")
            };

            if (isSelectable)
            {
                var capturedDate = date;
                cellButton.Clicked += (_, _) => OnCalendarDateSelected(capturedDate);
            }

            Grid.SetColumn(cellButton, cell % 7);
            Grid.SetRow(cellButton, cell / 7);
            gridCalendarDays.Children.Add(cellButton);
        }
    }

    // NEW
    private async void OnCalendarDateSelected(DateTime date)
    {
        _selectedDate = date;
        lblError.IsVisible = false;
        BuildCalendar(_calendarMonthAnchor);
        UpdateSummary();
        await LoadBookedSlotsAsync(date);
    }

    private async Task LoadBookedSlotsAsync(DateTime date)
    {
        try
        {
            var slots = await _dbService.GetBookedSlotsAsync(_caregiver.CaregiverID, date);

            if (slots.Count == 0)
            {
                borderBookedSlots.IsVisible = false;
                return;
            }

            borderBookedSlots.IsVisible = true;
            lblBookedSlots.Text = string.Join("\n", slots.Select(s => $"• {s.TimeRangeDisplay}"));
        }
        catch
        {
            borderBookedSlots.IsVisible = false;
        }
    }

    private void OnPrevMonthClicked(object sender, EventArgs e)
    {
        var candidate = _calendarMonthAnchor.AddMonths(-1);
        var firstSelectableMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        if (new DateTime(candidate.Year, candidate.Month, 1) < firstSelectableMonth)
            return;

        _calendarMonthAnchor = candidate;
        BuildCalendar(_calendarMonthAnchor);
    }

    private void OnNextMonthClicked(object sender, EventArgs e)
    {
        _calendarMonthAnchor = _calendarMonthAnchor.AddMonths(1);
        BuildCalendar(_calendarMonthAnchor);
    }

    // ───────────────────────── Summary / pricing ─────────────────────────

    private void UpdateSummary()
    {
        lblSummaryDate.Text = _selectedDate?.ToString("MMM dd, yyyy (ddd)") ?? "Not selected";
        lblSummaryRate.Text = $"₱{_caregiver.HourlyRate:0.00}/hour";
        lblSummaryFee.Text = $"₱{BookingFee:0.00}";

        var start = tpStartTime.Time;
        var end = tpEndTime.Time;
        lblSummaryTime.Text = $"{DateTime.Today.Add(start):h:mm tt} – {DateTime.Today.Add(end):h:mm tt}";

        var duration = end - start;
        if (duration <= TimeSpan.Zero)
        {
            lblSummaryDuration.Text = "—";
            lblSummaryTotal.Text = "₱0.00";
            return;
        }

        lblSummaryDuration.Text = $"{duration.TotalHours:0.##} hour(s)";

        var total = (decimal)duration.TotalHours * _caregiver.HourlyRate + BookingFee;
        lblSummaryTotal.Text = $"₱{total:0.00}";
    }

    // ───────────────────────── Confirm ─────────────────────────

    private async void OnConfirmBookingClicked(object sender, EventArgs e)
    {
        lblError.IsVisible = false;

        if (pckService.SelectedItem is not ServiceModel selectedService)
        {
            ShowError("Please select a service.");
            return;
        }

        if (_availableWeekdays.Count == 0)
        {
            ShowError("This caregiver hasn't set their availability yet.");
            return;
        }

        if (_selectedDate is not DateTime bookingDate)
        {
            ShowError("Please choose a date from the calendar.");
            return;
        }

        int weekdayIndex = DayOfWeekHelper.ToAppIndex(bookingDate.DayOfWeek);
        if (!_availableWeekdays.Contains(weekdayIndex))
        {
            ShowError("This caregiver is not available on the selected day.");
            return;
        }

        var startTime = tpStartTime.Time;
        var endTime = tpEndTime.Time;

        if (endTime <= startTime)
        {
            ShowError("End time must be after start time.");
            return;
        }

        if (startTime < _caregiver.AvailabilityStartTime || endTime > _caregiver.AvailabilityEndTime)
        {
            ShowError($"This caregiver is only available from {_caregiver.AvailabilityStartTime:hh\\:mm} to {_caregiver.AvailabilityEndTime:hh\\:mm}.");
            return;
        }

        bool hasConflict = await _dbService.HasConflictingBookingAsync(_caregiver.CaregiverID, bookingDate, startTime, endTime);
        if (hasConflict)
        {
            ShowError("This caregiver already has a booking during that time. Please choose a different time slot.");
            return;
        }

        if (bookingDate.Date == DateTime.Today && startTime <= DateTime.Now.TimeOfDay)
        {
            ShowError("Please choose a start time later than now.");
            return;
        }



        try
        {
            var total = (decimal)(endTime - startTime).TotalHours * _caregiver.HourlyRate + BookingFee;

            bool success = await _dbService.CreateBookingAsync(
                _userId,
                _caregiver.CaregiverID,
                selectedService.ServiceID,
                bookingDate,
                startTime,
                endTime,
                total);

            if (!success)
            {
                ShowError("This caregiver's profile could not be found. Please try again.");
                return;
            }

            await DisplayAlert(
                "Booking Requested",
                $"Your booking request with {_caregiver.FullName} for {bookingDate:MMM dd, yyyy} ({startTime:hh\\:mm}–{endTime:hh\\:mm}) has been sent!\nTotal: ₱{total:0.00}",
                "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        lblError.Text = message;
        lblError.IsVisible = true;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
