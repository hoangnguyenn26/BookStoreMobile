using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminUserListPage : ContentPage
{
    private readonly AdminUserListViewModel _viewModel;
    public AdminUserListPage(AdminUserListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}