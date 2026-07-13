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
        // Fire-and-forget is fine here: CreateTableAsync just needs to have
        // started before any page tries to query the Users table, and
        // EnsureInitializedAsync() inside DatabaseService guards against
        // running it twice.
        _ = AppServices.Database.InitializeAsync();

        return new Window(new NavigationPage(new MainPage())) { Title = "SwiftCare" };
    }
}
