using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class StockReceiptListPage : ContentPage
{
    private readonly StockReceiptListViewModel _viewModel;
    public StockReceiptListPage(StockReceiptListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as StockReceiptListViewModel)?.OnAppearing();
    }
}