using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Views; // Cần using namespace của Pages

namespace Bookstore.Mobile
{
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
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}", false);
        }

    }
}