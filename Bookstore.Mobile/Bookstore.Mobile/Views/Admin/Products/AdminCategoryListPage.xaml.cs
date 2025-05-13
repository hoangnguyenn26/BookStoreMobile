using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminCategoryListPage : ContentPage
{
    private readonly AdminCategoryListViewModel _viewModel;
    public AdminCategoryListPage(AdminCategoryListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminCategoryListViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}