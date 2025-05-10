using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminSupplierListViewModel : BaseViewModel
    {
        private readonly ISupplierApi _supplierApi;
        private readonly ILogger<AdminSupplierListViewModel> _logger;

        public AdminSupplierListViewModel(ISupplierApi supplierApi, ILogger<AdminSupplierListViewModel> logger)
        {
            _supplierApi = supplierApi;
            _logger = logger;
            Title = "Manage Suppliers";
            Suppliers = new ObservableCollection<SupplierDto>();
        }

        [ObservableProperty] private ObservableCollection<SupplierDto> _suppliers;
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && !HasError;

        [RelayCommand]
        private async Task LoadSuppliersAsync(object? parameter)
        {
            bool isRefreshing = parameter is bool b && b;
            if (IsBusy && !isRefreshing) return;
            IsBusy = true;
            try
            {
                if (isRefreshing) Suppliers.Clear();
                ErrorMessage = null;
                var response = await _supplierApi.GetAllSuppliers();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (!isRefreshing) Suppliers.Clear();
                    foreach (var supplier in response.Content.OrderByDescending(s => s.CreatedAtUtc))
                    {
                        Suppliers.Add(supplier);
                    }
                    _logger.LogInformation("Loaded {Count} suppliers.", Suppliers.Count);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load suppliers.";
                    _logger.LogWarning("Failed to load suppliers. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex) { ErrorMessage = ex.Message; _logger.LogError(ex, "Error loading suppliers."); }
            finally { IsBusy = false; OnPropertyChanged(nameof(ShowContent)); }
        }

        [RelayCommand] private async Task GoToAddSupplierAsync() => await Shell.Current.GoToAsync($"{nameof(AddEditSupplierPage)}?SupplierId={Guid.Empty}");
        [RelayCommand] private async Task GoToEditSupplierAsync(Guid? supplierId) { if (supplierId.HasValue) await Shell.Current.GoToAsync($"{nameof(AddEditSupplierPage)}?SupplierId={supplierId.Value}"); }

        [RelayCommand]
        private async Task DeleteSupplierAsync(Guid? supplierId)
        {
            if (!supplierId.HasValue || IsBusy) return;
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Delete this supplier?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                var response = await _supplierApi.DeleteSupplier(supplierId.Value);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Supplier {SupplierId} deleted.", supplierId.Value);
                    await LoadSuppliersAsync(true);
                }
                else
                {
                    string error = response.Error?.Content ?? "Failed to delete";
                    await DisplayAlertAsync("Error", error);
                    _logger.LogWarning("Failed to delete supplier {SupplierId}. Status: {StatusCode}", supplierId.Value, response.StatusCode);
                }
            }
            catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message); _logger.LogError(ex, "Error deleting supplier {SupplierId}", supplierId.Value); }
            finally { IsBusy = false; }
        }
        public void OnAppearing()
        {
            LoadSuppliersCommand.Execute(true);
        }
        public bool IsRefreshNeeded { get; set; } = true;
    }
} 