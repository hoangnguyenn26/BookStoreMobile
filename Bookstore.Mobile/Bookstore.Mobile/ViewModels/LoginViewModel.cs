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

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(LoginIdentifier) &&
            !string.IsNullOrWhiteSpace(Password) &&
            IsNotBusy;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            if (IsBusy || !CanLogin()) return; // Kiểm tra lại CanExecute ở đây

            IsBusy = true;
            ErrorMessage = null; // Xóa lỗi cũ

            try
            {
                _logger.LogInformation("Login attempt for: {LoginId}", LoginIdentifier);

                var loginRequest = new LoginRequestDto
                {
                    LoginIdentifier = LoginIdentifier!,
                    Password = Password!
                };

                // Gọi AuthService để thực hiện đăng nhập API
                var loginSuccess = await _authService.LoginAsync(loginRequest);

                if (loginSuccess)
                {
                    _logger.LogInformation("Login successful for {LoginId}. Navigating to main page.", LoginIdentifier);
                    await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                }
                else
                {
                    // Lấy thông báo lỗi từ AuthService
                    ErrorMessage = _authService.LastErrorMessage ?? "Invalid username or password.";
                    _logger.LogWarning("Login failed for {LoginId}: {Error}", LoginIdentifier, ErrorMessage);
                    await DisplayAlertAsync("Login Failed", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for {LoginId}", LoginIdentifier);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
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