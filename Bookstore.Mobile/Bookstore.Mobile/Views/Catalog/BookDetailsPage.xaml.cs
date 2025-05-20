using Bookstore.Mobile.ViewModels;
using System.Threading.Tasks;

namespace Bookstore.Mobile.Views;

public partial class BookDetailsPage : ContentPage
{
    public BookDetailsPage(BookDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}