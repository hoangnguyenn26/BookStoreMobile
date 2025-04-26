using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Bookstore.Mobile.ViewModels
{
    // Nhận AddressId dạng string từ query parameter
    [QueryProperty(nameof(AddressIdString), "AddressId")]
    public partial class AddEditAddressViewModel : BaseViewModel
    {
        private readonly IAddressApi _addressApi;
        private readonly ILogger<AddEditAddressViewModel> _logger;

        // Lưu AddressId thực tế (Guid) sau khi parse
        private Guid _actualAddressId = Guid.Empty;
        private string? _addressIdString;

        public AddEditAddressViewModel(IAddressApi addressApi, ILogger<AddEditAddressViewModel> logger)
        {
            _addressApi = addressApi;
            _logger = logger;
        }

        // ---- Properties cho Binding ----
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveAddressCommand))] private string? _street;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveAddressCommand))] private string? _village;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveAddressCommand))] private string? _district;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveAddressCommand))] private string? _city;
        [ObservableProperty] private bool _isDefault;
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && !HasError;

        // Property nhận giá trị string từ QueryProperty
        public string? AddressIdString
        {
            get => _addressIdString;
            set
            {
                if (_addressIdString != value)
                {
                    _addressIdString = value;
                    ProcessAddressId(value);
                }
            }
        }

        // Hàm xử lý Id dạng string nhận được
        private async void ProcessAddressId(string? idString)
        {
            _logger.LogInformation("Received AddressId string parameter: {AddressIdString}", idString);
            IsBusy = true;
            ErrorMessage = null;

            if (!string.IsNullOrEmpty(idString) && Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
            {
                _actualAddressId = parsedId;
                await LoadAddressDetailsAsync(parsedId);
            }
            else
            {
                _actualAddressId = Guid.Empty;
                Title = "Add New Address";
                // Reset các trường nhập liệu
                Street = Village = District = City = null;
                IsDefault = false;
                _logger.LogInformation("Processing as 'Add New Address'.");
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        // Hàm tải chi tiết địa chỉ (cho chế độ Edit)
        private async Task LoadAddressDetailsAsync(Guid addressIdToLoad)
        {
            Title = "Edit Address";
            try
            {
                var response = await _addressApi.GetAddressById(addressIdToLoad);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var address = response.Content;
                    Street = address.Street;
                    Village = address.Village;
                    District = address.District;
                    City = address.City;
                    IsDefault = address.IsDefault;
                    _logger.LogInformation("Address details loaded successfully for {AddressId}.", addressIdToLoad);
                }
                else
                {
                    ErrorMessage = "Failed to load address details.";
                    _logger.LogWarning("Failed to load address {AddressId} for editing. Status: {StatusCode}", addressIdToLoad, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading address details for {AddressId}", addressIdToLoad);
                ErrorMessage = "An error occurred while loading address.";
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        // --- Commands ---

        // Điều kiện để có thể nhấn nút Save
        private bool CanSaveAddress() =>
            !string.IsNullOrWhiteSpace(Street) &&
            !string.IsNullOrWhiteSpace(Village) &&
            !string.IsNullOrWhiteSpace(District) &&
            !string.IsNullOrWhiteSpace(City) &&
            IsNotBusy;

        // Lệnh thực hiện Lưu (Thêm mới hoặc Cập nhật)
        [RelayCommand(CanExecute = nameof(CanSaveAddress))]
        private async Task SaveAddressAsync()
        {
            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Attempting to save address (Actual Id: {AddressId})", _actualAddressId);

            try
            {
                bool success = false;
                Refit.ApiResponse<object>? response = null;
                var addressData = new CreateAddressDto
                {
                    Street = Street!,
                    Village = Village!,
                    District = District!,
                    City = City!,
                    IsDefault = IsDefault
                };

                if (_actualAddressId == Guid.Empty)
                {
                    var createDto = new CreateAddressDto
                    {
                        Street = Street!,
                        Village = Village!,
                        District = District!,
                        City = City!,
                        IsDefault = IsDefault
                    };
                    var createResponse = await _addressApi.CreateAddress(createDto);
                    success = createResponse.IsSuccessStatusCode;
                    if (!success) ErrorMessage = createResponse.Error?.Content ?? "Failed to create address";
                }
                else // Cập nhật
                {
                    var updateDto = new UpdateAddressDto
                    {
                        Street = Street!,
                        Village = Village!,
                        District = District!,
                        City = City!,
                        IsDefault = IsDefault
                    };
                    response = await _addressApi.UpdateAddress(_actualAddressId, updateDto);
                    success = response.IsSuccessStatusCode;
                    if (!success) ErrorMessage = response.Error?.Content ?? "Failed to update address";
                }

                if (success)
                {
                    _logger.LogInformation("Address saved successfully (Id: {AddressId})", _actualAddressId == Guid.Empty ? "(New)" : _actualAddressId.ToString());
                    await Shell.Current.GoToAsync($"//{nameof(AddressListPage)}");
                }
                else
                {
                    _logger.LogWarning("Failed to save address {AddressId}. Reason: {Reason}", _actualAddressId, ErrorMessage);
                    await DisplayAlertAsync("Save Failed", ErrorMessage ?? "Could not save the address.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while saving address {AddressId}", _actualAddressId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}