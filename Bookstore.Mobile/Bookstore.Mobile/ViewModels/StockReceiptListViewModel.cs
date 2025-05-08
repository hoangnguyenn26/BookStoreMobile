using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class StockReceiptListViewModel : BaseViewModel
    {
        private readonly IStockReceiptApi _receiptApi;
        private readonly ILogger<StockReceiptListViewModel> _logger;

        private int _currentPage = 1;
        private const int PageSize = 15;
        private bool _isLoadingMore = false;
        private bool _canLoadMore = true;
        private bool _hasLoaded = false;

        public StockReceiptListViewModel(IStockReceiptApi receiptApi, ILogger<StockReceiptListViewModel> logger)
        {
            _receiptApi = receiptApi;
            _logger = logger;
            Title = "Stock Receipts";
            Receipts = new ObservableCollection<StockReceiptDto>();
        }

        [ObservableProperty]
        private ObservableCollection<StockReceiptDto> _receipts;

        [RelayCommand]
        private async Task LoadReceiptsAsync(object forceRefreshObj = null)
        {
            // Default to false if not provided
            bool forceRefresh = false;

            // Handle different input types
            if (forceRefreshObj is bool b)
            {
                forceRefresh = b;
            }
            else if (forceRefreshObj is string str && bool.TryParse(str, out bool parsedBool))
            {
                forceRefresh = parsedBool;
            }
            else if (forceRefreshObj != null)
            {
                _logger.LogWarning($"Unexpected parameter type: {forceRefreshObj.GetType()}");
            }

            if (forceRefresh)
            {
                _currentPage = 1;
                _canLoadMore = true;
            }

            await RunSafeAsync(async () =>
            {
                var response = await _receiptApi.GetAllReceipts(_currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (forceRefresh)
                        Receipts.Clear();

                    foreach (var receipt in response.Content)
                        Receipts.Add(receipt);

                    _canLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load receipts.";
                    if (forceRefresh) Receipts.Clear();
                }
            }, showBusy: !_isLoadingMore);
        }

        [RelayCommand]
        private async Task LoadMoreReceiptsAsync()
        {
            if (_isLoadingMore || !_canLoadMore || IsBusy)
                return;

            _isLoadingMore = true;
            _currentPage++;

            try
            {
                await LoadReceiptsAsync(false);
            }
            finally
            {
                _isLoadingMore = false;
            }
        }

        [RelayCommand]
        private async Task GoToCreateReceiptAsync()
        {
            await Shell.Current.GoToAsync(nameof(CreateStockReceiptPage));
        }

        [RelayCommand]
        private async Task GoToReceiptDetailsAsync(StockReceiptDto? selectedReceipt)
        {
            if (selectedReceipt == null) return;
            await Shell.Current.GoToAsync($"{nameof(StockReceiptDetailsPage)}?ReceiptId={selectedReceipt.Id}");
        }

        public void OnAppearing()
        {
            if (!_hasLoaded)
            {
                _hasLoaded = true;
                LoadReceiptsCommand.ExecuteAsync(null);
            }
        }
    }
}