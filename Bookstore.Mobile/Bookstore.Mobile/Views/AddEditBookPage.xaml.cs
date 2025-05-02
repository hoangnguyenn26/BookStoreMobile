using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AddEditBookPage : ContentPage
{
    private readonly AddEditBookViewModel _viewModel;
    public AddEditBookPage(AddEditBookViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}