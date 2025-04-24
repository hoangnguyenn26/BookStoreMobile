using Bookstore.Mobile.Views;

namespace Bookstore.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Đăng ký các Route không có trong định nghĩa XAML hoặc cần đăng ký tường minh
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(BooksPage), typeof(BooksPage));
            Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
            Routing.RegisterRoute(nameof(BookDetailsPage), typeof(BookDetailsPage));
            //Routing.RegisterRoute(nameof(AddressListPage), typeof(AddressListPage)); 
        }
        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler != null)
            {
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}", false);
            }
        }
    }
}
