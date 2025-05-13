using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class WishlistPage : ContentPage
{
    public WishlistPage(WishlistViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is WishlistViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}