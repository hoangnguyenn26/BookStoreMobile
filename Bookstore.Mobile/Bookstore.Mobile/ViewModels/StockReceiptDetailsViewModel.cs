using Bookstore.Mobile.Interfaces.Apis; // IStockReceiptApi
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(ReceiptIdString), "ReceiptId")]
    public partial class StockReceiptDetailsViewModel : BaseViewModel
    {
        private readonly IStockReceiptApi _receiptApi;
        private readonly ILogger<StockReceiptDetailsViewModel> _logger;

        private Guid _actualReceiptId = Guid.Empty;
        private string? _receiptIdString;

        public StockReceiptDetailsViewModel(IStockReceiptApi receiptApi, ILogger<StockReceiptDetailsViewModel> logger)
        {
            _receiptApi = receiptApi;
            _logger = logger;
            Title = "Receipt Details";
        }

        public string? ReceiptIdString
        {
            get => _receiptIdString;
            set { if (_receiptIdString != value) { _receiptIdString = value; ProcessReceiptId(value); } }
        }

        [ObservableProperty] private StockReceiptDto? _receipt;
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && Receipt != null && !HasError;

        private async void ProcessReceiptId(string? idString)
        {
            IsBusy = true; ErrorMessage = null; Receipt = null;
            if (Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
            {
                _actualReceiptId = parsedId;
                await LoadReceiptDetailsAsync();
            }
            else
            {
                ErrorMessage = "Invalid Receipt ID received.";
                _logger.LogError("Invalid Receipt ID string received: {ReceiptIdString}", idString);
                IsBusy = false;
            }
            OnPropertyChanged(nameof(ShowContent));
            OnPropertyChanged(nameof(HasError));
        }

        [RelayCommand]
        private async Task LoadReceiptDetailsAsync()
        {
            if (_actualReceiptId == Guid.Empty)
            {
                if (!IsBusy) IsBusy = true;
                ErrorMessage = "Cannot load details: Receipt ID is invalid.";
                IsBusy = false;
                OnPropertyChanged(nameof(HasError)); OnPropertyChanged(nameof(ShowContent));
                return;
            }

            if (!IsBusy) IsBusy = true; // Đảm bảo IsBusy là true
            ErrorMessage = null;

            try
            {
                _logger.LogInformation("Loading stock receipt details for Id: {ReceiptId}", _actualReceiptId);
                // Gọi API GET /api/admin/stock-receipts/{id}
                var response = await _receiptApi.GetReceiptById(_actualReceiptId); // Cần Auth

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Receipt = response.Content;
                    Title = $"Receipt #{Receipt.Id.ToString().Substring(0, 8).ToUpper()}";
                    _logger.LogInformation("Stock receipt details loaded successfully.");
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed";
                    ErrorMessage = $"Error loading receipt details: {errorContent}";
                    _logger.LogWarning("Failed to load receipt {ReceiptId}. Status: {StatusCode}", _actualReceiptId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading receipt {ReceiptId}.", _actualReceiptId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public void OnAppearing()
        {
            // Load nếu chưa có dữ liệu và Id hợp lệ
            if (Receipt == null && !IsBusy && _actualReceiptId != Guid.Empty)
            {
                LoadReceiptDetailsCommand.Execute(null);
            }
        }
    }
}