using Microsoft.Maui.Controls;
using SwiftCare123P.Models;
using SwiftCare123P.Services;
using SwiftCare123P.ViewModels;

namespace SwiftCare123P;

public partial class CaregiverDashboard : ContentPage
{
    private readonly CaregiverDashboardViewModel _viewModel;

    public CaregiverDashboard()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _viewModel = new CaregiverDashboardViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}