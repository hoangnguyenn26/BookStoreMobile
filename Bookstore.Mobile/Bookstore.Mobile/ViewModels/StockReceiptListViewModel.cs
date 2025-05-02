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
        [NotifyPropertyChangedFor(nameof(HasError))]
        [NotifyPropertyChangedFor(nameof(ShowContent))]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && !HasError;

        [RelayCommand]
        private async Task LoadReceiptsAsync(object? parameter)
        {
            bool isRefreshing = parameter is bool b && b;
            if (_isLoadingMore || (!isRefreshing && !_canLoadMore)) return;
            if (!isRefreshing && IsBusy) return;

            IsBusy = true;
            if (isRefreshing)
            {
                _currentPage = 1;
                Receipts.Clear();
                _canLoadMore = true;
            }
            ErrorMessage = null;

            try
            {
                _logger.LogInformation("Loading stock receipts, Page: {Page}", _currentPage);
                var response = await _receiptApi.GetAllReceipts(_currentPage, PageSize);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (response.Content.Any())
                    {
                        foreach (var receipt in response.Content)
                            Receipts.Add(receipt);

                        _currentPage++;
                        _canLoadMore = response.Content.Count() == PageSize;
                    }
                    else
                    {
                        _canLoadMore = false;
                    }

                    _logger.LogInformation("Loaded {Count} receipts. Can load more: {CanLoadMore}",
                        response.Content?.Count() ?? 0, _canLoadMore);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load receipts";
                    _logger.LogWarning("Failed to load receipts. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading receipts";
                _logger.LogError(ex, "Error loading stock receipts");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        [RelayCommand]
        private async Task LoadMoreReceiptsAsync()
        {
            if (_isLoadingMore || !_canLoadMore || IsBusy) return;
            _isLoadingMore = true;
            _logger.LogInformation("LoadMoreReceiptAsync triggered.");
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
            _logger.LogInformation("Navigating to Create Stock Receipt Page.");
            await Shell.Current.GoToAsync(nameof(CreateStockReceiptPage));
        }

        [RelayCommand]
        private async Task GoToReceiptDetailsAsync(StockReceiptDto? selectedReceipt)
        {
            if (selectedReceipt == null) return;

            _logger.LogInformation("Navigating to Stock Receipt Details Id: {ReceiptId}", selectedReceipt.Id);
            await Shell.Current.GoToAsync($"{nameof(StockReceiptDetailsPage)}?ReceiptId={selectedReceipt.Id}");
        }

        public void OnAppearing()
        {
            if (Receipts.Count == 0)
                LoadReceiptsCommand.Execute(false);
        }
    }
}