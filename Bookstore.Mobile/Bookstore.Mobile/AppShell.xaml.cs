using Bookstore.Mobile.ViewModels;
using Bookstore.Mobile.Views;

namespace Bookstore.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            RegisterRoutes();
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
            Routing.RegisterRoute(nameof(WishlistPage), typeof(WishlistPage));

            //trang Admin/Staff
            Routing.RegisterRoute(nameof(AdminDashboardPage), typeof(AdminDashboardPage));
            Routing.RegisterRoute(nameof(AdminOrderListPage), typeof(AdminOrderListPage));
            Routing.RegisterRoute(nameof(AdminOrderDetailsPage), typeof(AdminOrderDetailsPage));
            Routing.RegisterRoute(nameof(AddressListPage), typeof(AddressListPage));
            Routing.RegisterRoute(nameof(AdminProductHomePage), typeof(AdminProductHomePage));
            Routing.RegisterRoute(nameof(AdminPromotionListPage), typeof(AdminPromotionListPage));
            Routing.RegisterRoute(nameof(AdminReportsPage), typeof(AdminReportsPage));
            Routing.RegisterRoute(nameof(AdminUserListPage), typeof(AdminUserListPage));
            Routing.RegisterRoute(nameof(AdminUserDetailsPage), typeof(AdminUserDetailsPage));
            Routing.RegisterRoute(nameof(CreateStockReceiptPage), typeof(CreateStockReceiptPage));
            Routing.RegisterRoute(nameof(StockReceiptListPage), typeof(StockReceiptListPage));
            Routing.RegisterRoute(nameof(StockReceiptDetailsPage), typeof(StockReceiptDetailsPage));

            Routing.RegisterRoute(nameof(AdminCategoryListPage), typeof(AdminCategoryListPage));
            Routing.RegisterRoute(nameof(AdminAuthorListPage), typeof(AdminAuthorListPage));
            Routing.RegisterRoute(nameof(AdminBookListPage), typeof(AdminBookListPage));
            Routing.RegisterRoute(nameof(AddEditCategoryPage), typeof(AddEditCategoryPage));
            Routing.RegisterRoute(nameof(AddEditAuthorPage), typeof(AddEditAuthorPage));
            Routing.RegisterRoute(nameof(AddEditBookPage), typeof(AddEditBookPage));
            Routing.RegisterRoute(nameof(InventoryAdjustmentPage), typeof(InventoryAdjustmentPage));
            Routing.RegisterRoute(nameof(AddEditPromotionPage), typeof(AddEditPromotionPage));
            Routing.RegisterRoute(nameof(AdminPromotionListPage), typeof(AdminPromotionListPage));

            // ... các trang quản lý khác
        }
    }
}
