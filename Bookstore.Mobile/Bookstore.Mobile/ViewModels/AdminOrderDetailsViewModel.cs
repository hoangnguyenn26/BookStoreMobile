using Bookstore.Mobile.Enums;
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

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
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && OrderDetails != null && !HasError;
        public bool HasShippingAddress => OrderDetails?.ShippingAddress != null;
        public decimal OrderSubtotal => OrderDetails?.OrderDetails?.Sum(d => d.UnitPrice * d.Quantity) ?? 0;

        [ObservableProperty] private ObservableCollection<OrderStatus> _availableStatuses;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(UpdateStatusCommand))] private OrderStatus? _selectedNewStatus;
        [ObservableProperty] private bool _isUpdatingStatus;
        [ObservableProperty] private string? _updateStatusMessage;
        [ObservableProperty] private Color? _updateStatusColor;
        public bool ShowUpdateStatusMessage => !string.IsNullOrEmpty(UpdateStatusMessage);
        public bool CanUpdateStatus => SelectedNewStatus.HasValue && OrderDetails != null && SelectedNewStatus.Value != OrderDetails.Status && !IsBusy && !IsUpdatingStatus;

        private async void ProcessOrderId(string? idString)
        {
            _logger.LogInformation("Received OrderId string parameter: {OrderIdString}", idString);
            IsBusy = true;
            ErrorMessage = null;
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
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(HasError));
            }
        }

        [RelayCommand]
        private async Task LoadOrderDetailsAsync()
        {
            if (_actualOrderId == Guid.Empty)
            {
                if (!IsBusy) IsBusy = true;
                ErrorMessage = "Cannot load details: Order ID is invalid.";
                IsBusy = false;
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(ShowContent));
                return;
            }

            if (!IsBusy) IsBusy = true;
            ErrorMessage = null;
            UpdateStatusMessage = null;

            try
            {
                _logger.LogInformation("Loading order details for Id: {OrderId}", _actualOrderId);
                var response = await _orderApi.GetOrderByIdForAdmin(_actualOrderId);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    OrderDetails = response.Content;
                    Title = $"Order #{OrderDetails.Id.ToString().Substring(0, 8).ToUpper()}";
                    SelectedNewStatus = OrderDetails.Status;
                    _logger.LogInformation("Order details loaded successfully for {OrderId}.", _actualOrderId);
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to load order details.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load order {OrderId}. Status: {StatusCode}, Reason: {Reason}", _actualOrderId, response.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading order {OrderId}.", _actualOrderId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(HasShippingAddress));
                OnPropertyChanged(nameof(OrderSubtotal));
                OnPropertyChanged(nameof(CanUpdateStatus));
                UpdateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand(CanExecute = nameof(CanUpdateStatus))]
        private async Task UpdateStatusAsync()
        {
            if (!SelectedNewStatus.HasValue || OrderDetails == null || _actualOrderId == Guid.Empty) return;

            IsUpdatingStatus = true;
            IsBusy = true; // Set general busy flag as well
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
                    //await LoadOrderDetailsAsync();
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to update status.";
                    UpdateStatusMessage = $"Error: {errorContent}";
                    UpdateStatusColor = Colors.Red;
                    _logger.LogWarning("Failed to update status for Order {OrderId}. Status: {StatusCode}, Reason: {Reason}", _actualOrderId, response.StatusCode, UpdateStatusMessage);
                    // Reset picker to original status?
                    SelectedNewStatus = OrderDetails.Status;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while updating status for Order {OrderId}", _actualOrderId);
                UpdateStatusMessage = "An unexpected error occurred.";
                UpdateStatusColor = Colors.Red;
                // Reset picker to original status on error
                if (OrderDetails != null) SelectedNewStatus = OrderDetails.Status;
            }
            finally
            {
                IsUpdatingStatus = false;
                IsBusy = false;
                OnPropertyChanged(nameof(ShowUpdateStatusMessage));
                UpdateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        public void OnAppearing()
        {
            if (OrderDetails == null && !IsBusy && _actualOrderId != Guid.Empty)
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