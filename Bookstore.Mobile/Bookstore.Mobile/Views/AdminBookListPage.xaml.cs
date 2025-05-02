using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminBookListPage : ContentPage
{
    private readonly AdminBookListViewModel _viewModel;
    public AdminBookListPage(AdminBookListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminBookListViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}