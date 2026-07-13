using SwiftCare123P.ViewModels;

namespace SwiftCare123P;

public partial class LoginPage : ContentPage
{
    private LoginViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel(Navigation);
        _viewModel.SetPage(this);
        BindingContext = _viewModel;
    }
}
