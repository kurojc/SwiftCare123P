using SwiftCare123P.ViewModels;

namespace SwiftCare123P;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel(Navigation);
    }
}
