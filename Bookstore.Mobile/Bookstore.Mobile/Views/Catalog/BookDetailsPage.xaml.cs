using Bookstore.Mobile.ViewModels;
using System.Threading.Tasks;

namespace Bookstore.Mobile.Views;

public partial class BookDetailsPage : ContentPage
{
    public BookDetailsPage(BookDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        WishlistButton.Clicked += WishlistButton_Clicked;
    }

    private async void WishlistButton_Clicked(object sender, EventArgs e)
    {
        // Animation scale
        await WishlistButton.ScaleTo(1.2, 100, Easing.CubicIn);
        await WishlistButton.ScaleTo(1.0, 100, Easing.CubicOut);
        // Gọi command sau animation
        if (BindingContext is BookDetailsViewModel vm && vm.ToggleWishlistCommand.CanExecute(null))
        {
            await vm.ToggleWishlistCommand.ExecuteAsync(null);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}