using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AddressListViewModel : BaseViewModel
    {
        private readonly IAddressApi _addressApi;
        private readonly IAuthService _authService;
        private readonly ILogger<AddressListViewModel> _logger;
        // private readonly INavigationService _navigationService;

        public AddressListViewModel(IAddressApi addressApi, IAuthService authService, ILogger<AddressListViewModel> logger/*, INavigationService navigationService*/)
        {
            _addressApi = addressApi;
            _authService = authService;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "My Addresses";
            Addresses = new ObservableCollection<AddressDto>();
        }

        [ObservableProperty]
        private ObservableCollection<AddressDto> _addresses;
        public bool HasItems => Addresses.Count > 0;


        // --- Commands ---
        [RelayCommand]
        private async Task LoadAddressesAsync()
        {
            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Loading user addresses.");
                var response = await _addressApi.GetAddresses();

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Addresses.Clear();
                    foreach (var address in response.Content)
                    {
                        Addresses.Add(address);
                    }
                    _logger.LogInformation("Loaded {Count} addresses.", Addresses.Count);
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to load addresses.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load addresses. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                }
            }, nameof(ShowContent));
            OnPropertyChanged(nameof(HasItems));
        }

        [RelayCommand]
        private async Task GoToAddAddressAsync()
        {
            _logger.LogInformation("Navigating to Add Address Page.");
            await Shell.Current.GoToAsync($"{nameof(AddEditAddressPage)}?AddressId={Guid.Empty}");
            // await _navigationService.NavigateToAsync(nameof(AddEditAddressPage), new Dictionary<string, object> { { "AddressId", Guid.Empty } });
        }

        [RelayCommand]
        private async Task GoToEditAddressAsync(Guid? addressId)
        {
            if (!addressId.HasValue || addressId.Value == Guid.Empty) return;
            _logger.LogInformation("Navigating to Edit Address Page for Id: {AddressId}", addressId.Value);
            await Shell.Current.GoToAsync($"{nameof(AddEditAddressPage)}?AddressId={addressId.Value}");
            // await _navigationService.NavigateToAsync(nameof(AddEditAddressPage), new Dictionary<string, object> { { "AddressId", addressId.Value } });
        }

        [RelayCommand]
        private async Task DeleteAddressAsync(Guid? addressId)
        {
            if (!addressId.HasValue || addressId.Value == Guid.Empty || IsBusy) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Are you sure you want to delete this address?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Deleting address with Id: {AddressId}", addressId.Value);
            try
            {
                // Gọi API DELETE /api/v1/user/addresses/{id}
                var response = await _addressApi.DeleteAddress(addressId.Value);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Address {AddressId} deleted successfully via API.", addressId.Value);
                    // Xóa khỏi ObservableCollection để cập nhật UI ngay lập tức
                    var itemToRemove = Addresses.FirstOrDefault(a => a.Id == addressId.Value);
                    if (itemToRemove != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => Addresses.Remove(itemToRemove));
                    }
                    OnPropertyChanged(nameof(HasItems));
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to delete address.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to delete address {AddressId}. Status: {StatusCode}, Reason: {Reason}", addressId.Value, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while deleting address {AddressId}", addressId.Value);
                ErrorMessage = $"Failed to delete address: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SetDefaultAsync(Guid? addressId)
        {
            if (!addressId.HasValue || addressId.Value == Guid.Empty || IsBusy) return;

            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Setting address {AddressId} as default.", addressId.Value);
            try
            {
                // Gọi API POST /api/v1/user/addresses/{id}/setdefault
                var response = await _addressApi.SetDefaultAddress(addressId.Value);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Address {AddressId} set as default successfully.", addressId.Value);
                    await LoadAddressesAsync();
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to set default address.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to set default address {AddressId}. Status: {StatusCode}, Reason: {Reason}", addressId.Value, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while setting default address {AddressId}", addressId.Value);
                ErrorMessage = $"Failed to set default address: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}