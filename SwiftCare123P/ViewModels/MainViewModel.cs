using System.Windows.Input;
using SwiftCare123P.MVVM;

namespace SwiftCare123P.ViewModels;

public class MainViewModel : BaseViewModel
{
    public ICommand GoToLoginCommand { get; }
    public ICommand GoToSignUpCommand { get; }

    public MainViewModel(INavigation navigation)
    {
        GoToLoginCommand = new AsyncRelayCommand(async _ => await navigation.PushAsync(new LoginPage()));
        GoToSignUpCommand = new AsyncRelayCommand(async _ => await navigation.PushAsync(new SignUpPage()));
    }
}
