using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Bookstore.Mobile.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterViewModel> _logger;

        public RegisterViewModel(IAuthService authService, ILogger<RegisterViewModel> logger)
        {
            Title = "Đăng ký";
            _authService = authService;
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
            // Xóa lỗi cũ
            FieldErrors.Clear();
            ErrorMessage = null;
            OnPropertyChanged(nameof(HasError));

            // Validation chi tiết phía client
            bool isValid = ValidateForm();

            if (!isValid)
            {
                // Hiển thị tổng hợp các lỗi trên UI
                var errorBuilder = new StringBuilder("Vui lòng kiểm tra lại thông tin:");
                foreach (var error in FieldErrors)
                {
                    errorBuilder.AppendLine($"\n• {error.Value}");
                }

                ErrorMessage = errorBuilder.ToString();
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
                var registerDto = new RegisterRequestDto
                {
                    UserName = UserName!.Trim(),
                    Email = Email!.Trim(),
                    Password = Password!,
                    ConfirmPassword = ConfirmPassword!,
                    FirstName = FirstName?.Trim(),
                    LastName = LastName?.Trim(),
                    PhoneNumber = PhoneNumber?.Trim()
                };

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
                    // Xử lý lỗi từ server
                    ProcessServerErrors();
                }
            }, nameof(ShowContent));
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Kiểm tra UserName
            if (string.IsNullOrWhiteSpace(UserName))
            {
                FieldErrors["UserName"] = "Tên đăng nhập không được để trống";
                isValid = false;
            }
            else if (UserName.Length < 3)
            {
                FieldErrors["UserName"] = "Tên đăng nhập phải có ít nhất 3 ký tự";
                isValid = false;
            }
            else if (UserName.Length > 30)
            {
                FieldErrors["UserName"] = "Tên đăng nhập không được vượt quá 30 ký tự";
                isValid = false;
            }
            else if (!Regex.IsMatch(UserName, @"^[a-zA-Z0-9._-]+$"))
            {
                FieldErrors["UserName"] = "Tên đăng nhập chỉ được chứa chữ cái, số, dấu chấm, dấu gạch ngang và gạch dưới";
                isValid = false;
            }

            // Kiểm tra Email
            if (string.IsNullOrWhiteSpace(Email))
            {
                FieldErrors["Email"] = "Email không được để trống";
                isValid = false;
            }
            else if (!new EmailAddressAttribute().IsValid(Email))
            {
                FieldErrors["Email"] = "Email không đúng định dạng";
                isValid = false;
            }

            // Kiểm tra Password
            if (string.IsNullOrWhiteSpace(Password))
            {
                FieldErrors["Password"] = "Mật khẩu không được để trống";
                isValid = false;
            }
            else if (Password.Length < 6)
            {
                FieldErrors["Password"] = "Mật khẩu phải có ít nhất 6 ký tự";
                isValid = false;
            }
            else if (Password.Length > 100)
            {
                FieldErrors["Password"] = "Mật khẩu không được vượt quá 100 ký tự";
                isValid = false;
            }

            // Kiểm tra ConfirmPassword
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                FieldErrors["ConfirmPassword"] = "Vui lòng xác nhận mật khẩu";
                isValid = false;
            }
            else if (Password != ConfirmPassword)
            {
                FieldErrors["ConfirmPassword"] = "Mật khẩu xác nhận không khớp";
                isValid = false;
            }

            // Kiểm tra PhoneNumber nếu được nhập
            if (!string.IsNullOrWhiteSpace(PhoneNumber) && !Regex.IsMatch(PhoneNumber, @"^[0-9+\-\s()]{6,20}$"))
            {
                FieldErrors["PhoneNumber"] = "Số điện thoại không hợp lệ";
                isValid = false;
            }

            return isValid;
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