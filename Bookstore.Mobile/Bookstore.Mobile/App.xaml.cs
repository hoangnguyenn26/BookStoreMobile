using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Views;

namespace Bookstore.Mobile
{
    public partial class App : Application
    {
        private readonly IAuthService _authService;
        public App(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await _authService.InitializeAsync();

            if (_authService.IsLoggedIn)
            {
                // await Shell.Current.GoToAsync($"//{nameof(HomePage)}"); // Cần đảm bảo Shell đã sẵn sàng
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }

        }
    }
}