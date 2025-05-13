using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Bookstore.Mobile.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ProfileViewModel> _logger;
        // private readonly INavigationService _navigationService;

        public ProfileViewModel(IAuthService authService, ILogger<ProfileViewModel> logger/*, INavigationService navigationService*/)
        {
            _authService = authService;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "My Profile";
            LoadUserInfo();
        }

        [ObservableProperty]
        private string? _userName;

        [ObservableProperty]
        private string? _email;

        [ObservableProperty]
        private string? _fullName;

        [ObservableProperty]
        private string? _phoneNumber;

        private void LoadUserInfo()
        {
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                var user = _authService.CurrentUser;
                UserName = user.UserName;
                Email = user.Email;
                FullName = $"{user.FirstName} {user.LastName}".Trim();
                PhoneNumber = user.PhoneNumber;
                _logger.LogInformation("User info loaded for profile: {Username}", UserName);
            }
            else
            {
                _logger.LogWarning("ProfileViewModel loaded but user is not logged in.");
                UserName = Email = FullName = PhoneNumber = null;
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            _logger.LogInformation("Initiating logout process...");

            try
            {
                await _authService.LogoutAsync();
                _logger.LogInformation("User logged out successfully.");

                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                await DisplayAlertAsync("Error", "Logout failed. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToAddressesAsync()
        {
            _logger.LogInformation("Navigating to Address List Page.");
            await Shell.Current.GoToAsync(nameof(AddressListPage));
            // await _navigationService.NavigateToAsync(nameof(AddressListPage));
        }

        [RelayCommand]
        private async Task GoToOrderHistoryAsync()
        {
            _logger.LogInformation("Navigating to Order History Page.");
            await Shell.Current.GoToAsync(nameof(OrderHistoryPage));
            // await _navigationService.NavigateToAsync(nameof(OrderHistoryPage));
        }

        // Hàm này có thể được gọi từ OnAppearing của Page nếu cần refresh thông tin
        public void RefreshUserInfo()
        {
            LoadUserInfo();
        }
    }
}