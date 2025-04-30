using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminOrderListPage : ContentPage
{
    private readonly AdminOrderListViewModel _viewModel;
    public AdminOrderListPage(AdminOrderListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}