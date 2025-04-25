using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class BookDetailsPage : ContentPage
{
    private readonly BookDetailsViewModel _viewModel;
    public BookDetailsPage(BookDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}