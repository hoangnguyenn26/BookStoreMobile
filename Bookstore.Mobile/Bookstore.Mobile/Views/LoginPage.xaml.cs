namespace Bookstore.Mobile.Views;
using Bookstore.Mobile.ViewModels;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    public LoginPage()
    {
        InitializeComponent();
    }
}