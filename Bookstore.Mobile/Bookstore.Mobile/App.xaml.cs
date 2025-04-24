
using Bookstore.Mobile.Interfaces.Services;

namespace Bookstore.Mobile
{
    // Chỉ định rõ ràng lớp Application của MAUI Controls
    public partial class App : Microsoft.Maui.Controls.Application
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
        }

    }
}