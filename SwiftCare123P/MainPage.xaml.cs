using SwiftCare123P.ViewModels;

namespace SwiftCare123P;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel(Navigation);
    }
}
