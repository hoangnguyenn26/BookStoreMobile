using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class AddEditPromotionPage : ContentPage
{
    private readonly AddEditPromotionViewModel _viewModel;
    public AddEditPromotionPage(AddEditPromotionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AddEditPromotionViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}