namespace Bookstore.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Đăng ký các Route không có trong định nghĩa XAML hoặc cần đăng ký tường minh
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage)); // Đăng ký để có thể GoToAsync("//LoginPage")
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(BooksPage), typeof(BooksPage));
            Routing.RegisterRoute(nameof(BookDetailsPage), typeof(BookDetailsPage));
            Routing.RegisterRoute(nameof(AddressListPage), typeof(AddressListPage)); // Ví dụ
        }
    }
}
