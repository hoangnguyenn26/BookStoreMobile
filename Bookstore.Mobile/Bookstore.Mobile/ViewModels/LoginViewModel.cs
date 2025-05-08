using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Bookstore.Mobile.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginViewModel> _logger;

        // Constructor nhận dependencies qua DI
        public LoginViewModel(IAuthService authService, ILogger<LoginViewModel> logger)
        {
            Title = "Login";
            _authService = authService;
            _logger = logger;
        }

        // --- Observable Properties cho Binding ---
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string? _loginIdentifier;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string? _password;

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(LoginIdentifier) &&
            !string.IsNullOrWhiteSpace(Password) &&
            IsNotBusy;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Login attempt for: {LoginId}", LoginIdentifier);
                var loginRequest = new LoginRequestDto
                {
                    LoginIdentifier = LoginIdentifier!,
                    Password = Password!
                };
                var loginSuccess = await _authService.LoginAsync(loginRequest);
                if (loginSuccess)
                {
                    _logger.LogInformation("Login successful for {LoginId}. Navigating to main page.", LoginIdentifier);
                    await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                }
                else
                {
                    ErrorMessage = _authService.LastErrorMessage ?? "Invalid username or password.";
                    _logger.LogWarning("Login failed for {LoginId}: {Error}", LoginIdentifier, ErrorMessage);
                    await DisplayAlertAsync("Login Failed", ErrorMessage);
                }
            }, nameof(ShowContent));
        }

        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            if (IsBusy) return;
            _logger.LogInformation("Navigating to Register Page");
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}