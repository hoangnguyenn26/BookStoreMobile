﻿using Bookstore.Mobile.Enums;
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(OrderIdString), "OrderId")]
    public partial class OrderDetailsViewModel : BaseViewModel
    {
        private readonly IOrderApi _orderApi;
        private readonly ILogger<OrderDetailsViewModel> _logger;

        private Guid _actualOrderId = Guid.Empty;
        private string? _orderIdString;

        public OrderDetailsViewModel(IOrderApi orderApi, ILogger<OrderDetailsViewModel> logger)
        {
            _orderApi = orderApi;
            _logger = logger;
            Title = "Order Details";
        }

        [ObservableProperty]
        private OrderDto? _orderDetails;

        public string? OrderIdString
        {
            get => _orderIdString;
            set
            {
                if (_orderIdString != value)
                {
                    _orderIdString = value;
                    ProcessOrderId(value);
                }
            }
        }
        public bool HasShippingAddress => OrderDetails?.ShippingAddress != null;
        public decimal OrderSubtotal => OrderDetails?.OrderDetails?.Sum(d => d.UnitPrice * d.Quantity) ?? 0;
        public bool CanCancelOrder => OrderDetails?.Status == OrderStatus.Pending && IsNotBusy;

        private async void ProcessOrderId(string? idString)
        {
            _logger.LogInformation("Received OrderId string parameter: {OrderIdString}", idString);
            ErrorMessage = null;
            OrderDetails = null;
            OnPropertyChanged(nameof(ShowContent));
            OnPropertyChanged(nameof(HasShippingAddress));
            OnPropertyChanged(nameof(OrderSubtotal));
            OnPropertyChanged(nameof(CanCancelOrder));

            if (!string.IsNullOrEmpty(idString) && Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
            {
                _actualOrderId = parsedId;
                Title = $"Order #{parsedId.ToString().Substring(0, 8).ToUpper()}";
                await LoadOrderDetailsAsync();
            }
            else
            {
                _actualOrderId = Guid.Empty;
                Title = "Order Details";
                ErrorMessage = "Invalid Order ID received.";
                _logger.LogWarning("Invalid Order ID string received: {OrderIdString}", idString);
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        [RelayCommand]
        private async Task LoadOrderDetailsAsync()
        {
            await RunSafeAsync(async () =>
            {
                if (_actualOrderId == Guid.Empty) return;
                _logger.LogInformation("Loading order details for Actual Id: {OrderId}", _actualOrderId);
                var response = await _orderApi.GetMyOrderById(_actualOrderId);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    OrderDetails = response.Content;
                    OnPropertyChanged(nameof(HasShippingAddress));
                    OnPropertyChanged(nameof(OrderSubtotal));
                    OnPropertyChanged(nameof(CanCancelOrder));
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load order details.";
                }
            }, nameof(ShowContent));
        }

        [RelayCommand(CanExecute = nameof(CanCancelOrder))]
        private async Task CancelOrderAsync()
        {
            if (OrderDetails == null) return;

            // Hỏi xác nhận người dùng
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Cancellation", "Are you sure you want to cancel this order?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Attempting to cancel order {OrderId}", _actualOrderId);
            try
            {
                // Gọi API PUT /api/v1/orders/{id}/cancel
                var response = await _orderApi.CancelMyOrder(_actualOrderId);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order {OrderId} cancelled successfully via API.", _actualOrderId);
                    await LoadOrderDetailsAsync();
                    await DisplayAlertAsync("Order Cancelled", "Your order has been successfully cancelled.", "OK");
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to cancel order.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to cancel order {OrderId}. Status: {StatusCode}, Reason: {Reason}", _actualOrderId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Cancellation Failed", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while cancelling order {OrderId}", _actualOrderId);
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