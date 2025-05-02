using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AddEditAuthorPage : ContentPage
{
    private readonly AddEditAuthorViewModel _viewModel;
    public AddEditAuthorPage(AddEditAuthorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}