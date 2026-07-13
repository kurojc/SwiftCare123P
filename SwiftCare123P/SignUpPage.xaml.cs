using SwiftCare123P.ViewModels;

namespace SwiftCare123P;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
        BindingContext = new SignUpViewModel(Navigation);
    }
}
