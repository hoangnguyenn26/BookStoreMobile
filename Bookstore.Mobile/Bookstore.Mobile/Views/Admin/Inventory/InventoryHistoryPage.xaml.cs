using Bookstore.Mobile.ViewModels.Admin.Inventory;

namespace Bookstore.Mobile.Views.Admin.Inventory;

public partial class InventoryHistoryPage : ContentPage
{
    private readonly InventoryHistoryViewModel _viewModel;
    public InventoryHistoryPage(InventoryHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}