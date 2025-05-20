using Bookstore.Mobile.Enums;
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Bookstore.Mobile.Helpers;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(OrderIdString), "OrderId")]
    public partial class AdminOrderDetailsViewModel : BaseViewModel
    {
        private readonly IOrderApi _orderApi;
        private readonly ILogger<AdminOrderDetailsViewModel> _logger;

        private Guid _actualOrderId = Guid.Empty;
        private string? _orderIdString;

        public AdminOrderDetailsViewModel(IOrderApi orderApi, ILogger<AdminOrderDetailsViewModel> logger)
        {
            _orderApi = orderApi ?? throw new ArgumentNullException(nameof(orderApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Title = "Order Details";
            AvailableStatuses = new ObservableCollection<OrderStatus>(
                Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList()
            );
        }

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

        [ObservableProperty] private OrderDto? _orderDetails;
        [ObservableProperty] private ObservableCollection<OrderStatus> _availableStatuses;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(UpdateStatusCommand))] private OrderStatus? _selectedNewStatus;
        [ObservableProperty] private bool _isUpdatingStatus;
        [ObservableProperty] private string? _updateStatusMessage;
        [ObservableProperty] private Color? _updateStatusColor;

        public override bool ShowContent => !IsBusy && OrderDetails != null && !HasError;

        public bool CanUpdateStatus =>
            SelectedNewStatus.HasValue &&
            OrderDetails != null &&
            SelectedNewStatus.Value != OrderDetails.Status &&
            IsNotBusy && !IsUpdatingStatus;

        public bool ShowUpdateStatusMessage => !string.IsNullOrEmpty(UpdateStatusMessage);

        [RelayCommand]
        private async Task LoadOrderDetailsAsync()
        {
            await RunSafeAsync(async () =>
            {
                if (_actualOrderId == Guid.Empty) return;
                var response = await _orderApi.GetOrderByIdForAdmin(_actualOrderId);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    OrderDetails = response.Content;
                    Title = $"Order #{OrderDetails.Id.ToString().Substring(0, 8).ToUpper()}";
                    SelectedNewStatus = OrderDetails.Status;
                }
                else
                {
                    ErrorMessage = ErrorMessageHelper.ToFriendlyErrorMessage(response.Error?.Content) ?? "Failed to load order details.";
                }
            }, nameof(ShowContent));
        }

        private async void ProcessOrderId(string? idString)
        {
            _logger.LogInformation("Received OrderId string parameter: {OrderIdString}", idString);
            await RunSafeAsync(async () =>
            {
                OrderDetails = null; // Clear previous details
                if (!string.IsNullOrEmpty(idString) && Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
                {
                    _actualOrderId = parsedId;
                    await LoadOrderDetailsAsync();
                }
                else
                {
                    _actualOrderId = Guid.Empty;
                    ErrorMessage = "Invalid Order ID received.";
                    _logger.LogError("Invalid Order ID string received: {OrderIdString}", idString);
                }
            }, nameof(ShowContent));
        }

        [RelayCommand(CanExecute = nameof(CanUpdateStatus))]
        private async Task UpdateStatusAsync()
        {
            if (!SelectedNewStatus.HasValue || OrderDetails == null || _actualOrderId == Guid.Empty) return;

            IsUpdatingStatus = true;
            UpdateStatusMessage = null;
            _logger.LogInformation("Admin attempting to update Order {OrderId} status to {NewStatus}", _actualOrderId, SelectedNewStatus.Value);

            try
            {
                var updateDto = new UpdateOrderStatusDto { NewStatus = SelectedNewStatus.Value };
                var response = await _orderApi.UpdateOrderStatus(_actualOrderId, updateDto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order {OrderId} status updated successfully to {NewStatus}", _actualOrderId, SelectedNewStatus.Value);
                    UpdateStatusMessage = "Status Updated Successfully!";
                    UpdateStatusColor = Colors.Green;

                    OrderDetails.Status = SelectedNewStatus.Value;
                    OnPropertyChanged(nameof(OrderDetails));
                }
                else
                {
                    string errorContent = ErrorMessageHelper.ToFriendlyErrorMessage(response.Error?.Content) ?? response.ReasonPhrase ?? "Failed to update status.";
                    UpdateStatusMessage = $"Error: {errorContent}";
                    UpdateStatusColor = Colors.Red;
                    _logger.LogWarning("Failed to update status for Order {OrderId}. Status: {StatusCode}, Reason: {Reason}", _actualOrderId, response.StatusCode, UpdateStatusMessage);
                    SelectedNewStatus = OrderDetails.Status;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while updating status for Order {OrderId}", _actualOrderId);
                UpdateStatusMessage = "An unexpected error occurred.";
                UpdateStatusColor = Colors.Red;
                if (OrderDetails != null) SelectedNewStatus = OrderDetails.Status;
            }
            finally
            {
                IsUpdatingStatus = false;
                OnPropertyChanged(nameof(ShowUpdateStatusMessage));
                UpdateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        public void OnAppearing()
        {
            if (OrderDetails == null && IsNotBusy && _actualOrderId != Guid.Empty)
            {
                LoadOrderDetailsCommand.Execute(null);
            }
            else if (OrderDetails != null)
            {
                UpdateStatusMessage = null;
                OnPropertyChanged(nameof(ShowUpdateStatusMessage));
            }
        }
    }
}