using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using Bookstore.Mobile.Helpers;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(SupplierIdString), "SupplierId")]
    public partial class AddEditSupplierViewModel : BaseViewModel
    {
        private readonly ISupplierApi _supplierApi;
        private readonly ILogger<AddEditSupplierViewModel> _logger;

        private Guid _actualSupplierId = Guid.Empty;
        private string? _supplierIdString;

        public AddEditSupplierViewModel(ISupplierApi supplierApi, ILogger<AddEditSupplierViewModel> logger)
        {
            _supplierApi = supplierApi;
            _logger = logger;
        }

        // Properties for Binding
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveSupplierCommand))] private string? _name;
        [ObservableProperty] private string? _contactPerson;
        [ObservableProperty] private string? _email;
        [ObservableProperty] private string? _phone;
        [ObservableProperty] private string? _address;

        public string? SupplierIdString
        {
            get => _supplierIdString;
            set { if (_supplierIdString != value) { _supplierIdString = value; ProcessSupplierId(value); } }
        }

        private async void ProcessSupplierId(string? idString)
        {
            IsBusy = true;
            ErrorMessage = null;
            try
            {
                if (Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
                {
                    _actualSupplierId = parsedId;
                    Title = "Edit Supplier";
                    await LoadSupplierDetailsAsync(parsedId);
                }
                else
                {
                    _actualSupplierId = Guid.Empty;
                    Title = "Add New Supplier";
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to initialize form";
                _logger.LogError(ex, "Error processing supplier ID");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(HasError));
            }
        }

        private void ResetForm()
        {
            Name = ContactPerson = Email = Phone = Address = null;
        }

        private async Task LoadSupplierDetailsAsync(Guid supplierIdToLoad)
        {
            try
            {
                var response = await _supplierApi.GetSupplierById(supplierIdToLoad);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var supplier = response.Content;
                    Name = supplier.Name;
                    ContactPerson = supplier.ContactPerson;
                    Email = supplier.Email;
                    Phone = supplier.Phone;
                    Address = supplier.Address;
                    ErrorMessage = null;
                    _logger.LogInformation("Loaded supplier details for {SupplierId}", supplierIdToLoad);
                }
                else
                {
                    ErrorMessage = ErrorMessageHelper.ToFriendlyErrorMessage(response.Error?.Content) ?? "Supplier not found.";
                    _logger.LogWarning("Failed to load supplier {SupplierId}. Status: {StatusCode}", supplierIdToLoad, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load supplier details.";
                _logger.LogError(ex, "Error loading supplier details for {SupplierId}", supplierIdToLoad);
            }
            finally
            {
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        private bool CanSaveSupplier() => !string.IsNullOrWhiteSpace(Name) && IsNotBusy;

        [RelayCommand(CanExecute = nameof(CanSaveSupplier))]
        private async Task SaveSupplierAsync()
        {
            IsBusy = true; ErrorMessage = null;
            try
            {
                bool success = false;
                ApiResponse<object>? response = null;
                ApiResponse<SupplierDto>? createResponse = null;

                if (_actualSupplierId == Guid.Empty) // Add
                {
                    var createDto = new CreateSupplierDto
                    {
                        Name = Name!,
                        ContactPerson = ContactPerson,
                        Email = Email,
                        Phone = Phone,
                        Address = Address
                    };
                    createResponse = await _supplierApi.CreateSupplier(createDto);
                    success = createResponse.IsSuccessStatusCode;
                    if (!success) ErrorMessage = ErrorMessageHelper.ToFriendlyErrorMessage(createResponse.Error?.Content) ?? "Failed";
                }
                else // Update
                {
                    var updateDto = new UpdateSupplierDto
                    {
                        Name = Name!,
                        ContactPerson = ContactPerson,
                        Email = Email,
                        Phone = Phone,
                        Address = Address
                    };
                    response = await _supplierApi.UpdateSupplier(_actualSupplierId, updateDto);
                    success = response.IsSuccessStatusCode;
                    if (!success) ErrorMessage = ErrorMessageHelper.ToFriendlyErrorMessage(response.Error?.Content) ?? "Failed";
                }

                if (success)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else { await DisplayAlertAsync("Save Failed", ErrorMessage ?? "Could not save."); }
            }
            catch (Exception ex) { ErrorMessage = "An error occurred while saving."; _logger.LogError(ex, "Error saving supplier"); await DisplayAlertAsync("Error", ErrorMessage); }
            finally { IsBusy = false; }
        }
    }
}