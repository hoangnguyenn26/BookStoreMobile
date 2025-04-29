using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Views;

namespace Bookstore.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Routes chính (Thường đã có trong XAML nhưng đăng ký lại không sao)
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
            //Routing.RegisterRoute(nameof(WishlistPage), typeof(WishlistPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));

            // Routes cho Authentication (Thường không nằm trong Flyout/Tabs)
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));

            // Routes cho luồng duyệt sản phẩm
            Routing.RegisterRoute(nameof(BooksPage), typeof(BooksPage));
            Routing.RegisterRoute(nameof(BookDetailsPage), typeof(BookDetailsPage));

            // Routes cho quản lý tài khoản người dùng
            Routing.RegisterRoute(nameof(AddressListPage), typeof(AddressListPage));
            Routing.RegisterRoute(nameof(AddEditAddressPage), typeof(AddEditAddressPage));

            // Routes cho luồng đặt hàng
            Routing.RegisterRoute(nameof(CheckoutPage), typeof(CheckoutPage));
            Routing.RegisterRoute(nameof(OrderDetailsPage), typeof(OrderDetailsPage));
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
    }
}
