using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class StockReceiptDetailsPage : ContentPage
{
    private readonly StockReceiptDetailsViewModel _viewModel;
    public StockReceiptDetailsPage(StockReceiptDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}