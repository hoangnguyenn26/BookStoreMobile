using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminReportsPage : ContentPage
{
    private readonly AdminReportsViewModel _viewModel;
    public AdminReportsPage(AdminReportsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminReportsViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}