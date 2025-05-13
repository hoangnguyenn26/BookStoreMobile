
using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class BooksPage : ContentPage
{
    private readonly BooksViewModel _viewModel;
    public BooksPage(BooksViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}