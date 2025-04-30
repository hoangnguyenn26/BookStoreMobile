using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Views;

namespace Bookstore.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var authService = IPlatformApplication.Current.Services.GetService<IAuthService>();
            if (authService != null && !authService.IsLoggedIn)
            {
                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
        }
        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(BooksPage), typeof(BooksPage));
            Routing.RegisterRoute(nameof(BookDetailsPage), typeof(BookDetailsPage));
            Routing.RegisterRoute(nameof(AddressListPage), typeof(AddressListPage));
            Routing.RegisterRoute(nameof(AddEditAddressPage), typeof(AddEditAddressPage));
            Routing.RegisterRoute(nameof(OrderHistoryPage), typeof(OrderHistoryPage));
            Routing.RegisterRoute(nameof(OrderDetailsPage), typeof(OrderDetailsPage));
            Routing.RegisterRoute(nameof(CheckoutPage), typeof(CheckoutPage));
            Routing.RegisterRoute(nameof(SubmitReviewPage), typeof(SubmitReviewPage));

            //trang Admin/Staff
            Routing.RegisterRoute(nameof(AdminDashboardPage), typeof(AdminDashboardPage));
            Routing.RegisterRoute(nameof(AdminOrderListPage), typeof(AdminOrderListPage));
            Routing.RegisterRoute(nameof(AdminOrderDetailsPage), typeof(AdminOrderDetailsPage));
            Routing.RegisterRoute(nameof(AddressListPage), typeof(AddressListPage));
            Routing.RegisterRoute(nameof(AdminProductHomePage), typeof(AdminProductHomePage));
            Routing.RegisterRoute(nameof(AdminPromotionListPage), typeof(AdminPromotionListPage));
            Routing.RegisterRoute(nameof(AdminReportsPage), typeof(AdminReportsPage));
            Routing.RegisterRoute(nameof(AdminUserListPage), typeof(AdminUserListPage));
            Routing.RegisterRoute(nameof(CreateStockReceiptPage), typeof(CreateStockReceiptPage));
            Routing.RegisterRoute(nameof(StockReceiptListPage), typeof(StockReceiptListPage));

            // ... các trang quản lý khác
        }
    }
}
