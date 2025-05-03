using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class CreateStockReceiptPage : ContentPage
{
    private readonly CreateStockReceiptViewModel _viewModel;
    public CreateStockReceiptPage(CreateStockReceiptViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}