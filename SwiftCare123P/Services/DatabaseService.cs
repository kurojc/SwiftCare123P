using SQLite;
using SwiftCare123P.Common;
using SwiftCare123P.Models;

namespace SwiftCare123P.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _db;

    // Fixed service list — matches the web app's checkboxes/dropdowns exactly.
    private static readonly string[] DefaultServiceNames =
    {
        "Child Care", "Elderly Care", "Pet Care", "House Sitting", "Special Needs Care"
    };

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db is not null)
            return _db;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "swiftcare.db3");
        _db = new SQLiteAsyncConnection(dbPath);

        await _db.CreateTableAsync<User>();
        await _db.CreateTableAsync<CaregiverProfileEntity>();
        await _db.CreateTableAsync<BookingEntity>();
        await _db.CreateTableAsync<ReviewEntity>();
        await _db.CreateTableAsync<ServiceEntity>();

        await SeedServicesAsync(_db);

        return _db;
    }

    private static async Task SeedServicesAsync(SQLiteAsyncConnection db)
    {
        var count = await db.Table<ServiceEntity>().CountAsync();
        if (count > 0) return;

        foreach (var name in DefaultServiceNames)
            await db.InsertAsync(new ServiceEntity { ServiceName = name });
    }

    public async Task InitializeAsync() => await GetDbAsync();

    // ───────────────────────── Users ─────────────────────────

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var db = await GetDbAsync();
        return await db.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
        => await GetUserByEmailAsync(email) is not null;

    public async Task SaveUserAsync(User user)
    {
        var db = await GetDbAsync();
        user.AccountStatus = string.IsNullOrWhiteSpace(user.AccountStatus) ? "Active" : user.AccountStatus;
        if (user.CreatedDate == default) user.CreatedDate = DateTime.UtcNow;

        // AutoIncrement: sqlite-net writes the generated Id back onto `user` after insert.
        await db.InsertAsync(user);
    }

    // ───────────────────── CareSeeker dashboard ─────────────────────

    public async Task<List<CaregiverModel>> GetCaregiversAsync(string searchName = "", string searchService = "", int userId = 0)
    {
        var db = await GetDbAsync();
        var profiles = await db.Table<CaregiverProfileEntity>().ToListAsync();

        var result = new List<CaregiverModel>();
        foreach (var p in profiles)
        {
            var user = await db.Table<User>().Where(u => u.Id == p.UserID).FirstOrDefaultAsync();
            result.Add(await ToCaregiverModelAsync(db, p, user));
        }

        if (!string.IsNullOrWhiteSpace(searchName))
            result = result.Where(c => c.FullName?.Contains(searchName, StringComparison.OrdinalIgnoreCase) == true).ToList();

        if (!string.IsNullOrWhiteSpace(searchService))
            result = result.Where(c => c.ServicesOffered?.Contains(searchService, StringComparison.OrdinalIgnoreCase) == true).ToList();

        return result;
    }

    public async Task<List<BookingModel>> GetUserBookingsAsync(int userId)
    {
        var db = await GetDbAsync();
        var bookings = await db.Table<BookingEntity>().Where(b => b.UserID == userId).ToListAsync();

        var result = new List<BookingModel>();
        foreach (var b in bookings.OrderByDescending(x => x.BookingDate))
            result.Add(await ToBookingModelAsync(db, b, forCaregiverView: false));

        return result;
    }

    public async Task<UserModel?> GetUserProfileAsync(int userId)
    {
        var db = await GetDbAsync();
        var user = await db.Table<User>().Where(u => u.Id == userId).FirstOrDefaultAsync();
        if (user is null) return null;

        return new UserModel
        {
            UserID = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ContactNo = user.ContactNumber,
            Gender = user.Gender,
            Birthdate = user.Birthdate,
            Address = user.Address
        };
    }

    public async Task<bool> UpdateUserProfileAsync(UserModel user)
    {
        var db = await GetDbAsync();
        var existing = await db.Table<User>().Where(u => u.Id == user.UserID).FirstOrDefaultAsync();
        if (existing is null) return false;

        existing.FirstName = user.FirstName ?? existing.FirstName;
        existing.LastName = user.LastName ?? existing.LastName;
        existing.ContactNumber = user.ContactNo ?? existing.ContactNumber;
        existing.Gender = user.Gender ?? existing.Gender;
        existing.Address = user.Address ?? existing.Address;
        if (user.Birthdate.HasValue) existing.Birthdate = user.Birthdate.Value;

        await db.UpdateAsync(existing);
        return true;
    }

    // ───────────────────── Caregiver profile ─────────────────────

    public async Task<CaregiverModel?> GetCaregiverProfileAsync(int caregiverId)
    {
        var db = await GetDbAsync();
        var entity = await db.Table<CaregiverProfileEntity>().Where(c => c.UserID == caregiverId).FirstOrDefaultAsync();
        if (entity is null) return null;

        var user = await db.Table<User>().Where(u => u.Id == caregiverId).FirstOrDefaultAsync();
        return await ToCaregiverModelAsync(db, entity, user);
    }

    public async Task UpdateCaregiverProfileAsync(CaregiverModel profile)
    {
        var db = await GetDbAsync();
        var entity = await GetOrCreateProfileByUserIdAsync(db, profile.CaregiverID);

        entity.HourlyRate = profile.HourlyRate;
        entity.Bio = profile.Bio ?? string.Empty;
        entity.ServicesOffered = profile.ServicesOffered ?? string.Empty;
        await db.UpdateAsync(entity);

        if (!string.IsNullOrWhiteSpace(profile.FullName))
        {
            var user = await db.Table<User>().Where(u => u.Id == profile.CaregiverID).FirstOrDefaultAsync();
            if (user is not null)
            {
                var parts = profile.FullName.Trim().Split(' ', 2);
                user.FirstName = parts[0];
                user.LastName = parts.Length > 1 ? parts[1] : string.Empty;
                if (!string.IsNullOrWhiteSpace(profile.ContactNo)) user.ContactNumber = profile.ContactNo;
                if (!string.IsNullOrWhiteSpace(profile.Address)) user.Address = profile.Address;
                await db.UpdateAsync(user);
            }
        }
    }

    public async Task UpdateCaregiverAvailabilityAsync(int caregiverId, string selectedDays, TimeSpan startTime, TimeSpan endTime)
    {
        var db = await GetDbAsync();
        var entity = await GetOrCreateProfileByUserIdAsync(db, caregiverId);

        entity.AvailableDays = selectedDays ?? string.Empty;
        entity.DayStartTime = startTime.ToString(@"hh\:mm");
        entity.DayEndTime = endTime.ToString(@"hh\:mm");
        entity.AvailabilityStatus = string.IsNullOrWhiteSpace(selectedDays) ? "Unavailable" : "Available";

        await db.UpdateAsync(entity);
    }

    private static async Task<CaregiverProfileEntity> GetOrCreateProfileByUserIdAsync(SQLiteAsyncConnection db, int userId)
    {
        var entity = await db.Table<CaregiverProfileEntity>().Where(c => c.UserID == userId).FirstOrDefaultAsync();
        if (entity is not null) return entity;

        entity = new CaregiverProfileEntity { UserID = userId, AvailabilityStatus = "Unavailable" };
        await db.InsertAsync(entity);
        return entity;
    }

    // ───────────────────────── Bookings ─────────────────────────

    public async Task<List<BookingModel>> GetCaregiverBookingsAsync(int caregiverId)
    {
        var db = await GetDbAsync();
        var profile = await db.Table<CaregiverProfileEntity>().Where(c => c.UserID == caregiverId).FirstOrDefaultAsync();
        if (profile is null) return new List<BookingModel>();

        var bookings = await db.Table<BookingEntity>().Where(b => b.CaregiverID == profile.CaregiverID).ToListAsync();

        var result = new List<BookingModel>();
        foreach (var b in bookings.OrderByDescending(x => x.BookingDate))
            result.Add(await ToBookingModelAsync(db, b, forCaregiverView: true));

        return result;
    }

    public async Task UpdateBookingStatusAsync(int bookingId, string status)
    {
        var db = await GetDbAsync();
        var booking = await db.Table<BookingEntity>().Where(b => b.BookingID == bookingId).FirstOrDefaultAsync();
        if (booking is null) return;

        booking.Status = status;
        await db.UpdateAsync(booking);
    }

    public async Task<bool> CreateBookingAsync(int userId, int caregiverId, int serviceId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime)
    {
        var db = await GetDbAsync();

        // caregiverId here is the caregiver's UserID (same convention used throughout this
        // service); BookingEntity.CaregiverID actually points at CaregiverProfileEntity's own
        // primary key, so resolve that first — same lookup GetCaregiverBookingsAsync uses.
        var profile = await db.Table<CaregiverProfileEntity>().Where(c => c.UserID == caregiverId).FirstOrDefaultAsync();
        if (profile is null) return false;

        var booking = new BookingEntity
        {
            UserID = userId,
            CaregiverID = profile.CaregiverID,
            ServiceID = serviceId,
            BookingDate = bookingDate.Date,
            StartTime = startTime.ToString(@"hh\:mm"),
            EndTime = endTime.ToString(@"hh\:mm"),
            Status = BookingStatus.Pending
        };

        await db.InsertAsync(booking);
        return true;
    }

    // ───────────────────────── Services ─────────────────────────

    public async Task<List<ServiceModel>> GetServicesAsync()
    {
        var db = await GetDbAsync();
        var services = await db.Table<ServiceEntity>().ToListAsync();

        return services
            .OrderBy(s => s.ServiceID)
            .Select(s => new ServiceModel { ServiceID = s.ServiceID, ServiceName = s.ServiceName })
            .ToList();
    }

    // ───────────────────────── Reviews ─────────────────────────

    public async Task<List<ReviewModel>> GetCaregiverReviewsAsync(int caregiverId)
    {
        var db = await GetDbAsync();
        var profile = await db.Table<CaregiverProfileEntity>().Where(c => c.UserID == caregiverId).FirstOrDefaultAsync();
        if (profile is null) return new List<ReviewModel>();

        var reviews = await db.Table<ReviewEntity>().Where(r => r.CaregiverID == profile.CaregiverID).ToListAsync();

        var result = new List<ReviewModel>();
        foreach (var r in reviews.OrderByDescending(x => x.ReviewDate))
        {
            var client = await db.Table<User>().Where(u => u.Id == r.UserID).FirstOrDefaultAsync();
            var booking = await db.Table<BookingEntity>().Where(b => b.BookingID == r.BookingID).FirstOrDefaultAsync();
            var service = booking is null ? null : await db.Table<ServiceEntity>().Where(s => s.ServiceID == booking.ServiceID).FirstOrDefaultAsync();

            var stars = Math.Clamp(r.Rating, 0, 5);

            result.Add(new ReviewModel
            {
                ReviewID = r.ReviewID,
                BookingID = r.BookingID,
                UserID = r.UserID,
                CaregiverID = r.CaregiverID,
                Rating = r.Rating,
                ReviewText = r.Comment,
                ClientName = client is null ? "Anonymous" : $"{client.FirstName} {client.LastName}",
                ServiceName = service?.ServiceName ?? string.Empty,
                ReviewDate = r.ReviewDate.ToString("MMM dd, yyyy"),
                Stars = new string('★', stars) + new string('☆', 5 - stars)
            });
        }

        return result;
    }

    // ───────────────────────── Mapping helpers ─────────────────────────

    private static async Task<CaregiverModel> ToCaregiverModelAsync(SQLiteAsyncConnection db, CaregiverProfileEntity entity, User? user)
    {
        var reviews = await db.Table<ReviewEntity>().Where(r => r.CaregiverID == entity.CaregiverID).ToListAsync();
        var avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

        var bio = entity.Bio ?? string.Empty;
        var fullName = user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim();

        return new CaregiverModel
        {
            CaregiverID = user?.Id ?? entity.UserID, // exposed as the caregiver's UserID throughout this app
            FullName = fullName,
            Address = user?.Address ?? string.Empty,
            ContactNo = user?.ContactNumber ?? string.Empty,
            HourlyRate = entity.HourlyRate,
            HourlyRateDisplay = $"₱{entity.HourlyRate:0.00}/Hour",
            AvailabilityStatus = entity.AvailabilityStatus,
            AvailableDays = entity.AvailableDays,
            AvailabilityStartTime = ParseTimeOfDay(entity.DayStartTime, new TimeSpan(8, 0, 0)),
            AvailabilityEndTime = ParseTimeOfDay(entity.DayEndTime, new TimeSpan(17, 0, 0)),
            Bio = bio,
            ShortBio = bio.Length > 60 ? bio[..60] + "..." : bio,
            ServicesOffered = entity.ServicesOffered,
            AvgRating = avgRating
        };
    }

    private static async Task<BookingModel> ToBookingModelAsync(SQLiteAsyncConnection db, BookingEntity b, bool forCaregiverView)
    {
        var service = await db.Table<ServiceEntity>().Where(s => s.ServiceID == b.ServiceID).FirstOrDefaultAsync();

        string otherPartyName;
        if (forCaregiverView)
        {
            var client = await db.Table<User>().Where(u => u.Id == b.UserID).FirstOrDefaultAsync();
            otherPartyName = client is null ? "Unknown" : $"{client.FirstName} {client.LastName}";
        }
        else
        {
            var profile = await db.Table<CaregiverProfileEntity>().Where(c => c.CaregiverID == b.CaregiverID).FirstOrDefaultAsync();
            var caregiverUser = profile is null ? null : await db.Table<User>().Where(u => u.Id == profile.UserID).FirstOrDefaultAsync();
            otherPartyName = caregiverUser is null ? "Unknown" : $"{caregiverUser.FirstName} {caregiverUser.LastName}";
        }

        var start = TimeSpan.TryParse(b.StartTime, out var s) ? s : TimeSpan.Zero;
        var end = TimeSpan.TryParse(b.EndTime, out var e) ? e : TimeSpan.Zero;
        var hasReview = await db.Table<ReviewEntity>().Where(r => r.BookingID == b.BookingID).CountAsync() > 0;
        var (bg, fg) = StatusColors(b.Status);

        return new BookingModel
        {
            BookingID = b.BookingID,
            CaregiverID = b.CaregiverID,
            CaregiverName = otherPartyName,
            ServiceName = service?.ServiceName ?? string.Empty,
            BookingDate = b.BookingDate,
            StartTime = start,
            EndTime = end,
            Status = b.Status,
            HasReview = hasReview,
            DateTimeDisplay = $"{b.BookingDate:MMM dd, yyyy}\n{start:hh\\:mm} – {end:hh\\:mm}",
            StatusColor = bg,
            StatusTextColor = fg,
            CanReview = !forCaregiverView && b.Status == BookingStatus.Completed && !hasReview,

            // Caregiver-side booking actions: only ever offered to the caregiver's own view,
            // and only for the status they're actually valid from.
            CanAccept = forCaregiverView && b.Status == BookingStatus.Pending,
            CanDecline = forCaregiverView && b.Status == BookingStatus.Pending,
            CanComplete = forCaregiverView && b.Status == BookingStatus.Confirmed
        };
    }

    private static TimeSpan ParseTimeOfDay(string? value, TimeSpan fallback)
        => TimeSpan.TryParse(value, out var result) ? result : fallback;

    private static (Color bg, Color fg) StatusColors(string status) => status switch
    {
        BookingStatus.Confirmed => (Color.FromArgb("#e0f7fa"), Color.FromArgb("#006064")),
        BookingStatus.Completed => (Color.FromArgb("#e8f5e9"), Color.FromArgb("#2e7d32")),
        BookingStatus.Cancelled => (Color.FromArgb("#fdecea"), Color.FromArgb("#b71c1c")),
        _ => (Color.FromArgb("#fff8e1"), Color.FromArgb("#8d6e00")), // Pending / default
    };
}