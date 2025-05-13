using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminSupplierListPage : ContentPage
{
    private readonly AdminSupplierListViewModel _viewModel;
    public AdminSupplierListPage(AdminSupplierListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminSupplierListViewModel vm)
        {
            vm.OnAppearing();
        }
    }
} 