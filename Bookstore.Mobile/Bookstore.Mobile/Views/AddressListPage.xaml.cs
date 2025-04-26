using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AddressListPage : ContentPage
{
    private readonly AddressListViewModel _viewModel;
    public AddressListPage(AddressListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadAddressesCommand.CanExecute(null))
        {
            _viewModel.LoadAddressesCommand.Execute(null);
        }
    }
}