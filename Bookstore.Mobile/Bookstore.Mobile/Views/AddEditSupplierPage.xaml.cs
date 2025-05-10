using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AddEditSupplierPage : ContentPage
{
    private readonly AddEditSupplierViewModel _viewModel;
    public AddEditSupplierPage(AddEditSupplierViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AddEditSupplierViewModel vm)
        {
            // No OnAppearing logic needed for now, but can be added if required
        }
    }
} 