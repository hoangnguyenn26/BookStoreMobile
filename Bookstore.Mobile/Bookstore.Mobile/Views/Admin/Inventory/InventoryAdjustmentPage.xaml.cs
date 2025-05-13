using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class InventoryAdjustmentPage : ContentPage
{
    private readonly InventoryAdjustmentViewModel _viewModel;
    public InventoryAdjustmentPage(InventoryAdjustmentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}