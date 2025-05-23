using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Bookstore.Mobile.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterViewModel> _logger;
        private readonly IValidator<RegisterRequestDto> _registerValidator;

        public RegisterViewModel(IAuthService authService, ILogger<RegisterViewModel> logger, IValidator<RegisterRequestDto> registerValidator)
        {
            Title = "Đăng ký";
            _authService = authService;
            _logger = logger;
            _registerValidator = registerValidator;
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
        private Dictionary<string, string> _fieldErrors = new Dictionary<string, string>();

        public override bool HasError => !string.IsNullOrEmpty(ErrorMessage) || FieldErrors.Any();

        private bool CanRegister() =>
            !string.IsNullOrWhiteSpace(UserName) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(ConfirmPassword) &&
            IsNotBusy;

        public string UserNameError => FieldErrors.ContainsKey("UserName") ? FieldErrors["UserName"] : string.Empty;
        public string EmailError => FieldErrors.ContainsKey("Email") ? FieldErrors["Email"] : string.Empty;
        public string PasswordError => FieldErrors.ContainsKey("Password") ? FieldErrors["Password"] : string.Empty;
        public string ConfirmPasswordError => FieldErrors.ContainsKey("ConfirmPassword") ? FieldErrors["ConfirmPassword"] : string.Empty;
        public string PhoneNumberError => FieldErrors.ContainsKey("PhoneNumber") ? FieldErrors["PhoneNumber"] : string.Empty;

        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            FieldErrors.Clear();
            ErrorMessage = null;
            OnPropertyChanged(nameof(HasError));

            var registerDto = new RegisterRequestDto
            {
                UserName = UserName?.Trim(),
                Email = Email?.Trim(),
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                FirstName = FirstName?.Trim(),
                LastName = LastName?.Trim(),
                PhoneNumber = PhoneNumber?.Trim()
            };

            // Validate bằng FluentValidation
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    FieldErrors[error.PropertyName] = error.ErrorMessage;
                }
                ErrorMessage = "Vui lòng kiểm tra lại thông tin:\n" + string.Join("\n", validationResult.Errors.Select(e => "• " + e.ErrorMessage));
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(UserNameError));
                OnPropertyChanged(nameof(EmailError));
                OnPropertyChanged(nameof(PasswordError));
                OnPropertyChanged(nameof(ConfirmPasswordError));
                OnPropertyChanged(nameof(PhoneNumberError));
                return;
            }

            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Registration attempt for {Username}", UserName);

                var success = await _authService.RegisterAsync(registerDto);

                if (success)
                {
                    _logger.LogInformation("Registration successful for {Username}. Navigating to Login.", UserName);
                    await DisplayAlertAsync("Đăng ký thành công", "Đăng ký tài khoản thành công! Vui lòng đăng nhập.", "OK");
                    await Shell.Current.GoToAsync($"///{nameof(LoginPage)}");
                }
                else
                {
                    ProcessServerErrors();
                }
            }, nameof(ShowContent));
        }

        private void ProcessServerErrors()
        {
            var errorMessage = _authService.LastErrorMessage;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Xử lý phản hồi lỗi từ server
                if (errorMessage.Contains("\n"))
                {
                    var lines = errorMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var errorBuilder = new StringBuilder();

                    // Map lỗi vào từng trường cụ thể
                    foreach (var line in lines)
                    {
                        string trimmedLine = line.Trim();
                        errorBuilder.AppendLine($"• {trimmedLine}");

                        // Phân loại lỗi theo từng trường
                        if (trimmedLine.Contains("Email") || trimmedLine.Contains("email"))
                            FieldErrors["Email"] = trimmedLine;
                        else if (trimmedLine.Contains("Mật khẩu") && !trimmedLine.Contains("Xác nhận"))
                            FieldErrors["Password"] = trimmedLine;
                        else if (trimmedLine.Contains("Tên đăng nhập") || trimmedLine.Contains("UserName"))
                            FieldErrors["UserName"] = trimmedLine;
                        else if (trimmedLine.Contains("Xác nhận mật khẩu") || trimmedLine.Contains("ConfirmPassword"))
                            FieldErrors["ConfirmPassword"] = trimmedLine;
                        else if (trimmedLine.Contains("Số điện thoại") || trimmedLine.Contains("PhoneNumber"))
                            FieldErrors["PhoneNumber"] = trimmedLine;
                    }

                    ErrorMessage = "Vui lòng kiểm tra lại thông tin:\n" + errorBuilder.ToString();
                }
                else
                {
                    ErrorMessage = errorMessage;
                }
            }
            else
            {
                ErrorMessage = "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin của bạn hoặc thử lại sau.";
            }

            _logger.LogWarning("Registration failed for {Username}: {Error}", UserName, ErrorMessage);
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(UserNameError));
            OnPropertyChanged(nameof(EmailError));
            OnPropertyChanged(nameof(PasswordError));
            OnPropertyChanged(nameof(ConfirmPasswordError));
            OnPropertyChanged(nameof(PhoneNumberError));
        }

        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            if (IsBusy) return;
            _logger.LogInformation("Navigating to Login Page");
            await Shell.Current.GoToAsync($"///{nameof(LoginPage)}");
        }
    }
}