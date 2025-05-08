using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class CartPage : ContentPage
{
    private readonly CartViewModel _viewModel;
    public CartPage(CartViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CartViewModel vm)
            vm.OnAppearing();
    }
}