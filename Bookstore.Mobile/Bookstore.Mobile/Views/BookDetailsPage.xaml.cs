using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class BookDetailsPage : ContentPage
{
    public BookDetailsPage(BookDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}