
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
namespace Bookstore.Mobile.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterViewModel> _logger;

        public RegisterViewModel(IAuthService authService, /*INavigationService navigationService,*/ ILogger<RegisterViewModel> logger)
        {
            Title = "Register";
            _authService = authService;
            // _navigationService = navigationService;
            _logger = logger;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string? _userName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string? _email;

        [ObservableProperty]
        private string? _firstName;

        [ObservableProperty]
        private string? _lastName;

        [ObservableProperty]
        private string? _phoneNumber;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string? _password;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string? _confirmPassword;

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private bool CanRegister() =>
            !string.IsNullOrWhiteSpace(UserName) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password) &&
            Password == ConfirmPassword &&
            IsNotBusy;

        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            if (IsBusy || !CanRegister()) return;
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                // Tạo DTO request
                var registerDto = new RegisterRequestDto
                {
                    UserName = UserName!,
                    Email = Email!,
                    Password = Password!,
                    ConfirmPassword = ConfirmPassword!,
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = PhoneNumber
                };

                _logger.LogInformation("Registration attempt for {Username}", UserName);

                // Gọi AuthService để thực hiện đăng ký API
                var success = await _authService.RegisterAsync(registerDto);

                if (success)
                {
                    _logger.LogInformation("Registration successful for {Username}. Navigating back to Login.", UserName);
                    await DisplayAlertAsync("Success", "Registration successful! Please log in.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = _authService.LastErrorMessage ?? "Registration failed. Please check your input.";
                    _logger.LogWarning("Registration failed for {Username}: {Error}", UserName, ErrorMessage);
                    await DisplayAlertAsync("Registration Failed", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during registration for {Username}", UserName);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            if (IsBusy) return;
            _logger.LogInformation("Navigating back to Login Page");
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}