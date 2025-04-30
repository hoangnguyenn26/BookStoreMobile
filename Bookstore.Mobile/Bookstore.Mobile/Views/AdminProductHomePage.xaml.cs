using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminProductHomePage : ContentPage
{
    private readonly AdminProductHomeViewModel _viewModel;
    public AdminProductHomePage(AdminProductHomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}