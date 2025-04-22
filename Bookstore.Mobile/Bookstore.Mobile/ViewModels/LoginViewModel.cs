
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
namespace Bookstore.Mobile.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        // Inject các service cần thiết (sẽ thêm sau)
        // private readonly IAuthService _authService;
        // private readonly INavigationService _navigationService;
        private readonly ILogger<LoginViewModel> _logger;
        public LoginViewModel(ILogger<LoginViewModel> logger/*IAuthService authService, INavigationService navigationService*/)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Title = "Login";
            // _authService = authService;
            // _navigationService = navigationService;
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

        // Lệnh thực thi việc đăng nhập
        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = null;

            try
            {
                // TODO: Ngày 9 - Gọi AuthService để thực hiện đăng nhập API
                await Task.Delay(2000); // Giả lập việc gọi API
                _logger.LogInformation("Login attempt for: {LoginId}", LoginIdentifier); // Cần inject ILogger

                // Giả sử đăng nhập thành công
                // await _navigationService.NavigateToAsync("//HomePage");

                // Giả sử đăng nhập thất bại
                // throw new AuthenticationException("Invalid username or password.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {LoginId}", LoginIdentifier);
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        //[RelayCommand]
        //private async Task GoToRegisterAsync()
        //{
        //    if (IsBusy) return;
        //    _logger.LogInformation("Navigating to Register Page");
        //    // TODO: Ngày 9 - Sử dụng NavigationService để điều hướng
        //    // await _navigationService.NavigateToAsync(nameof(RegisterPage));
        //    await Shell.Current.GoToAsync(nameof(RegisterPage));
        //}


    }
}