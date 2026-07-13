using SwiftCare123P.Services;

namespace SwiftCare123P;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _ = Task.Run(async () => await AppServices.Database.InitializeAsync());

        return new Window(new NavigationPage(new MainPage())) { Title = "SwiftCare" };
    }
}
