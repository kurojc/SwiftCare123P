using System.Windows.Input;
using SwiftCare123P.MVVM;
using SwiftCare123P.Services;

namespace SwiftCare123P.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly INavigation _navigation;
    private readonly IDatabaseService _databaseService;
    private Page? _page;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LogInCommand { get; }
    public ICommand BackToHomeCommand { get; }
    public ICommand GoToSignUpCommand { get; }

    public LoginViewModel(INavigation navigation, IDatabaseService? databaseService = null)
    {
        _navigation = navigation;
        _databaseService = databaseService ?? AppServices.Database;

        LogInCommand = new AsyncRelayCommand(async _ => await LogInAsync());
        BackToHomeCommand = new AsyncRelayCommand(async _ => await _navigation.PopAsync());
        GoToSignUpCommand = new AsyncRelayCommand(async _ => await _navigation.PushAsync(new SignUpPage()));
    }

    public void SetPage(Page page) => _page = page;

    private async Task LogInAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter both email and password.";
            return;
        }

        var user = await _databaseService.GetUserByEmailAsync(Email.Trim());

        if (user is null || user.Password != Password)
        {
            ErrorMessage = "Invalid email or password.";
            return;
        }

        ErrorMessage = string.Empty;
        
        // Store user info for dashboard
        await SecureStorage.Default.SetAsync("UserID", user.Id.ToString());
        await SecureStorage.Default.SetAsync("UserName", $"{user.FirstName} {user.LastName}");
        await SecureStorage.Default.SetAsync("UserRole", user.Role);

        // Show alert and wait for it to complete
        if (_page is not null)
        {
            await _page.DisplayAlert("Log In", $"Welcome back, {user.FirstName}!", "OK");
        }

        // Navigate to appropriate dashboard based on role
        if (user.Role == "CareSeeker")
        {
            await _navigation.PushAsync(new UserDashboad());
        }
        else if (user.Role == "Caregiver")
        {
            await _navigation.PushAsync(new CaregiverDashboard());
        }
    }
}
