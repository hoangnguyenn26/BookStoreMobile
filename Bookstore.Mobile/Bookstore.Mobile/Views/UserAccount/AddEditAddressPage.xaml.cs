using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AddEditAddressPage : ContentPage
{
    public AddEditAddressPage(AddEditAddressViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}