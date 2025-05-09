using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AdminPromotionListPage : ContentPage
{
    private readonly AdminPromotionListViewModel _viewModel;
    public AdminPromotionListPage(AdminPromotionListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminPromotionListViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}