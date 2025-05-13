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

        [ObservableProperty]
        private bool _isLoadingMore = false;

        [ObservableProperty]
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

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private StockReceiptDto? _selectedReceipt;

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
                CanLoadMore = true;
            }

            IsRefreshing = true;
            await RunSafeAsync(async () =>
            {
                var response = await _receiptApi.GetAllReceipts(_currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (forceRefresh)
                        Receipts.Clear();

                    foreach (var receipt in response.Content)
                        Receipts.Add(receipt);

                    CanLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load receipts. Please check your connection and try again.";
                    if (forceRefresh) Receipts.Clear();
                }
            }, showBusy: !IsLoadingMore);
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task LoadMoreReceiptsAsync()
        {
            if (IsLoadingMore || !CanLoadMore || IsBusy)
                return;

            IsLoadingMore = true;
            _currentPage++;

            try
            {
                await LoadReceiptsAsync(false);
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        [RelayCommand]
        private async Task GoToCreateReceiptAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(CreateStockReceiptPage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to Create Receipt page");
                await DisplayAlertAsync("Navigation Error", "Unable to navigate to the Create Receipt page. Please try again.");
            }
        }

        [RelayCommand]
        private async Task GoToReceiptDetailsAsync(StockReceiptDto? selectedReceipt)
        {
            if (selectedReceipt == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(StockReceiptDetailsPage)}?ReceiptId={selectedReceipt.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error navigating to Receipt details page for receipt {selectedReceipt.Id}");
                await DisplayAlertAsync("Navigation Error", "Unable to view receipt details. Please try again.");
            }
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