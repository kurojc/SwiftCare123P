using SwiftCare123P.ViewModels;

namespace SwiftCare123P;

public partial class SignUpPage : ContentPage
{
    private SignUpViewModel _viewModel;

    public SignUpPage()
    {
        InitializeComponent();
        _viewModel = new SignUpViewModel(Navigation);
        _viewModel.SetPage(this);
        BindingContext = _viewModel;
    }
}
