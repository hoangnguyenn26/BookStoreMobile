using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminUserDetailsPage : ContentPage
{
    private readonly AdminUserDetailsViewModel _viewModel;
    public AdminUserDetailsPage(AdminUserDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminUserDetailsViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}