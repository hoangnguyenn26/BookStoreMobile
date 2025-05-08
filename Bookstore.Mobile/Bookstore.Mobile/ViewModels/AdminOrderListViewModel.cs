using Bookstore.Mobile.Enums;
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminOrderListViewModel : BaseViewModel
    {
        private readonly IOrderApi _orderApi;
        private int _currentPage = 1;
        private const int PageSize = 15;
        private bool _canLoadMore = true;

        public ObservableCollection<string> AvailableStatusFilters { get; } = new ObservableCollection<string>();

        public AdminOrderListViewModel(IOrderApi adminOrderApi)
        {
            _orderApi = adminOrderApi;
            Title = "All Orders";
            Orders = new ObservableCollection<OrderSummaryDto>();

            // Khởi tạo danh sách filter
            AvailableStatusFilters.Add("All");
            foreach (var status in Enum.GetNames(typeof(OrderStatus)))
            {
                AvailableStatusFilters.Add(status);
            }
        }

        [ObservableProperty]
        private ObservableCollection<OrderSummaryDto> _orders;

        [ObservableProperty]
        private string? _searchTerm;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadOrdersCommand))]
        private string _selectedStatusFilter = "All";

        public bool ShowContent => !IsBusy && !HasError;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        [RelayCommand]
        private async Task LoadOrders(bool isRefreshing = false)
        {
            if (IsBusy && !isRefreshing)
                return;

            try
            {
                IsBusy = true;
                if (isRefreshing || SelectedStatusFilter != "All")  // Reset page if filter changes
                {
                    _currentPage = 1;
                    Orders.Clear();
                    _canLoadMore = true;
                }

                OrderStatus? status = null;
                if (_selectedStatusFilter != "All" && Enum.TryParse<OrderStatus>(_selectedStatusFilter, out var parsedStatus))
                {
                    status = parsedStatus;
                }

                var response = await _orderApi.GetAllOrdersForAdmin(_currentPage, PageSize, status);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var order in response.Content)
                        Orders.Add(order);

                    _currentPage++;
                    _canLoadMore = response.Content.Count() == PageSize;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadMore()
        {
            if (!_canLoadMore || IsBusy) return;
            await LoadOrders();
        }

        [RelayCommand]
        private async Task Refresh() => await LoadOrders(true);

        [RelayCommand]
        private async Task GoToOrderDetails(OrderSummaryDto order)
        {
            if (order != null)
            {
                await Shell.Current.GoToAsync($"{nameof(AdminOrderDetailsPage)}?OrderId={order.Id}");
            }
        }

        public void OnAppearing() => LoadOrdersCommand.Execute(false);
    }
}