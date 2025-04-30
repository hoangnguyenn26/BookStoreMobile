using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views
{
    public partial class AdminOrderListPage : ContentPage
    {
        public AdminOrderListPage(AdminOrderListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AdminOrderListViewModel vm)
            {
                vm.OnAppearing();
            }
        }
    }
}