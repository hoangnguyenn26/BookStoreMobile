using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Views;

namespace Bookstore.Mobile;

public partial class App : Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        MainPage = IPlatformApplication.Current.Services.GetRequiredService<AppShell>();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await _authService.InitializeAsync();
        await WaitForShellReady();
        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }

    private async Task WaitForShellReady()
    {
        while (MainPage == null || Shell.Current == null)
        {
            await Task.Delay(50);
        }
    }
}