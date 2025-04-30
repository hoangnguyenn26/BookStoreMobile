using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminOrderDetailsPage : ContentPage
{
    private readonly AdminOrderDetailsViewModel _viewModel;
    public AdminOrderDetailsPage(AdminOrderDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}