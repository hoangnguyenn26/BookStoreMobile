using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminAuthorListPage : ContentPage
{
    private readonly AdminAuthorListViewModel _viewModel;
    public AdminAuthorListPage(AdminAuthorListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminAuthorListViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}