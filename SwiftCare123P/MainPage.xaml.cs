using SwiftCare123P.ViewModels;

namespace SwiftCare123P;
// dustin push test
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel(Navigation);
    }
}
