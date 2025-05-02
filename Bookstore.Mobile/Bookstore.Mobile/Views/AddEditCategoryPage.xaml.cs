using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AddEditCategoryPage : ContentPage
{
    private readonly AddEditCategoryViewModel _viewModel;
    public AddEditCategoryPage(AddEditCategoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}