using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class OrderDetailsPage : ContentPage
{
    private readonly OrderDetailsViewModel _viewModel;
    public OrderDetailsPage(OrderDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}