using System.Windows.Input;
using SwiftCare123P.MVVM;
using SwiftCare123P.Models;
using SwiftCare123P.Services;

namespace SwiftCare123P.ViewModels;

public class SignUpViewModel : BaseViewModel
{
    private readonly INavigation _navigation;
    private readonly IDatabaseService _databaseService;
    private Page? _page;

    // ----- Role toggle -----
    private bool _isNeedCaregiverSelected = true;
    public bool IsNeedCaregiverSelected
    {
        get => _isNeedCaregiverSelected;
        set
        {
            if (SetProperty(ref _isNeedCaregiverSelected, value) && value)
                IsBeCaregiverSelected = false;
        }
    }

    private bool _isBeCaregiverSelected;
    public bool IsBeCaregiverSelected
    {
        get => _isBeCaregiverSelected;
        set
        {
            if (SetProperty(ref _isBeCaregiverSelected, value) && value)
                IsNeedCaregiverSelected = false;
        }
    }

    // ----- Form fields -----
    private string _firstName = string.Empty;
    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

    private string _lastName = string.Empty;
    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _contactNumber = string.Empty;
    public string ContactNumber { get => _contactNumber; set => SetProperty(ref _contactNumber, value); }

    private DateTime _birthdate = DateTime.Today.AddYears(-18);
    public DateTime Birthdate { get => _birthdate; set => SetProperty(ref _birthdate, value); }

    private string _gender = string.Empty;
    public string Gender { get => _gender; set => SetProperty(ref _gender, value); }

    private string _address = string.Empty;
    public string Address { get => _address; set => SetProperty(ref _address, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

    private string _errorMessage = string.Empty;
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

    // ----- Commands -----
    public ICommand SelectNeedCaregiverCommand { get; }
    public ICommand SelectBeCaregiverCommand { get; }
    public ICommand CreateAccountCommand { get; }
    public ICommand BackToHomeCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public SignUpViewModel(INavigation navigation, IDatabaseService? databaseService = null)
    {
        _navigation = navigation;
        _databaseService = databaseService ?? AppServices.Database;

        SelectNeedCaregiverCommand = new RelayCommand(_ => IsNeedCaregiverSelected = true);
        SelectBeCaregiverCommand = new RelayCommand(_ => IsBeCaregiverSelected = true);

        CreateAccountCommand = new AsyncRelayCommand(async _ => await CreateAccountAsync());
        BackToHomeCommand = new AsyncRelayCommand(async _ => await _navigation.PopToRootAsync());
        GoToLoginCommand = new AsyncRelayCommand(async _ => await _navigation.PushAsync(new SignUpPage()));
    }

    public void SetPage(Page page) => _page = page;

    private async Task CreateAccountAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please fill in all required fields.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (await _databaseService.EmailExistsAsync(Email.Trim()))
        {
            ErrorMessage = "An account with this email already exists.";
            return;
        }

        var userRole = IsBeCaregiverSelected ? "Caregiver" : "CareSeeker";

        var newUser = new User
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Email = Email.Trim(),
            ContactNumber = ContactNumber.Trim(),
            Birthdate = Birthdate,
            Gender = Gender,
            Address = Address.Trim(),
            Password = Password,
            Role = userRole
        };

        await _databaseService.SaveUserAsync(newUser);


        ErrorMessage = string.Empty;
        
        // Store user info for dashboard
        await SecureStorage.Default.SetAsync("UserID", newUser.Id.ToString());
        await SecureStorage.Default.SetAsync("UserName", $"{newUser.FirstName} {newUser.LastName}");
        await SecureStorage.Default.SetAsync("UserRole", userRole);

        // Show alert and wait for it to complete
        if (_page is not null)
        {
            await _page.DisplayAlert("Account Created", $"Welcome, {newUser.FirstName}!", "OK");
        }

        // Navigate to appropriate dashboard based on role
        if (userRole == "CareSeeker")
        {
            await _navigation.PushAsync(new UserDashboard());
        }
        else if (userRole == "Caregiver")
        {
            await _navigation.PushAsync(new CaregiverDashboard());
        }
    }
}
