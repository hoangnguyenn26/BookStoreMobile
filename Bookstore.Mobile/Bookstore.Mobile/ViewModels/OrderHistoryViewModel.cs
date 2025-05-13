// Bookstore.Mobile/ViewModels/OrderHistoryViewModel.cs
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class OrderHistoryViewModel : BaseViewModel
    {
        private readonly IOrderApi _orderApi;
        private readonly ILogger<OrderHistoryViewModel> _logger;
        // private readonly INavigationService _navigationService;

        private int _currentPage = 1;
        private const int PageSize = 15;
        private bool _isLoadingMore = false;
        private bool _canLoadMore = true;

        public OrderHistoryViewModel(IOrderApi orderApi, ILogger<OrderHistoryViewModel> logger /*, INavigationService navigationService*/)
        {
            _orderApi = orderApi;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "Order History";
            Orders = new ObservableCollection<OrderSummaryDto>();
        }

        [ObservableProperty]
        private ObservableCollection<OrderSummaryDto> _orders;

        [RelayCommand(CanExecute = nameof(CanLoadMoreOrders))]
        private async Task LoadMoreOrdersAsync()
        {
            if (_isLoadingMore) return;
            _isLoadingMore = true;
            await RunSafeAsync(async () =>
            {
                var response = await _orderApi.GetAllOrdersForAdmin(_currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var order in response.Content)
                    {
                        Orders.Add(order);
                    }
                    _currentPage++;
                    _canLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load order history.";
                }
            }, nameof(ShowContent));
            _isLoadingMore = false;
        }

        private bool CanLoadMoreOrders() => _canLoadMore && IsNotBusy;

        [RelayCommand]
        private async Task RefreshOrdersAsync()
        {
            _logger.LogInformation("RefreshOrdersCommand triggered.");
            // Đặt lại _canLoadMore trước khi load lại trang đầu
            _canLoadMore = true;
            await LoadOrdersInternalAsync(isRefreshing: true);
        }

        private async Task LoadOrdersInternalAsync(bool isRefreshing)
        {
            IsBusy = true;
            ErrorMessage = null;

            if (isRefreshing)
            {
                _currentPage = 1;
            }

            try
            {
                _logger.LogInformation("Loading orders (Internal). Refreshing: {IsRefreshing}, Page: {Page}", isRefreshing, _currentPage);
                var response = await _orderApi.GetMyOrders(_currentPage, PageSize);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (isRefreshing)
                    {
                        MainThread.BeginInvokeOnMainThread(() => Orders.Clear());
                    }

                    if (response.Content.Any())
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            foreach (var order in response.Content)
                            {
                                Orders.Add(order);
                            }
                        });
                        _currentPage++;
                        _canLoadMore = response.Content.Count() == PageSize;
                        _logger.LogInformation("Loaded {Count} orders. Can load more: {CanLoadMore}", response.Content.Count(), _canLoadMore);
                    }
                    else
                    {
                        _canLoadMore = false;
                        _logger.LogInformation("No more orders found on page {Page}.", _currentPage);
                        // Nếu là trang đầu tiên mà không có gì, Orders sẽ rỗng (EmptyView tự hiển thị)
                    }
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to load orders.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load orders. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                    if (isRefreshing) _canLoadMore = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading orders.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                if (isRefreshing) _canLoadMore = true;
            }
            finally
            {
                IsBusy = false;
                _isLoadingMore = false;
                LoadMoreOrdersCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(ShowContent));
            }
        }


        [RelayCommand]
        private async Task GoToOrderDetailsAsync(OrderSummaryDto? selectedOrder)
        {
            if (selectedOrder == null || IsBusy) return;
            _logger.LogInformation("Navigating to Order Details for Id: {OrderId}", selectedOrder.Id);
            await Shell.Current.GoToAsync($"{nameof(OrderDetailsPage)}?OrderId={selectedOrder.Id}");
            // await _navigationService.NavigateToAsync(nameof(OrderDetailsPage), new Dictionary<string, object> { { "OrderId", selectedOrder.Id } });
        }

        [RelayCommand]
        private async Task GoShoppingAsync()
        {
            _logger.LogInformation("Navigating back to shop from empty order history.");
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        public void OnAppearing()
        {
            // Gọi command Refresh nếu danh sách rỗng
            if (Orders.Count == 0 && RefreshOrdersCommand.CanExecute(null))
            {
                RefreshOrdersCommand.Execute(null);
            }
        }
    }
}