using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    // Model phụ để quản lý trạng thái Selected cho RadioButton
    public partial class SelectableAddressDto : ObservableObject
    {
        [ObservableProperty]
        private AddressDto _address; // Địa chỉ gốc

        [ObservableProperty]
        private bool _isSelected;

        public SelectableAddressDto(AddressDto address)
        {
            Address = address;
        }
    }

    public partial class CheckoutViewModel : BaseViewModel
    {
        private readonly ICartApi _cartApi;
        private readonly IAddressApi _addressApi;
        private readonly IOrderApi _orderApi;
        private readonly ILogger<CheckoutViewModel> _logger;
        // private readonly INavigationService _navigationService;

        public CheckoutViewModel(ICartApi cartApi, IAddressApi addressApi, IOrderApi orderApi, ILogger<CheckoutViewModel> logger/*, INavigationService navigationService*/)
        {
            _cartApi = cartApi;
            _addressApi = addressApi;
            _orderApi = orderApi;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "Checkout";
            Addresses = new ObservableCollection<SelectableAddressDto>();
        }

        // --- Observable Properties ---
        [ObservableProperty]
        private ObservableCollection<SelectableAddressDto> _addresses;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanPlaceOrder))] // Cập nhật CanPlaceOrder khi thay đổi
        private SelectableAddressDto? _selectedAddress; // Địa chỉ đang được chọn

        [ObservableProperty] private decimal _subtotal;
        [ObservableProperty] private decimal _shippingFee = 0;
        [ObservableProperty] private decimal _discount = 0;
        [ObservableProperty] private decimal _grandTotal;
        // Cờ trạng thái loading riêng
        [ObservableProperty] private bool _isLoadingAddresses;
        [ObservableProperty] private bool _isLoadingCart;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isPlacingOrder; // Khi nhấn nút Place Order

        // Thuộc tính tính toán
        public bool HasAddresses => Addresses.Count > 0;
        public bool NoAddressesFound => !IsLoadingAddresses && !HasAddresses;
        public bool ShowContent => !IsLoadingAddresses && !IsLoadingCart && !HasError;
        public bool CanPlaceOrder => SelectedAddress != null && Subtotal > 0 && IsNotBusy; // Điều kiện để đặt hàng

        // --- Commands ---
        [RelayCommand]
        private async Task LoadCheckoutDataAsync()
        {
            // Load đồng thời Cart và Address
            if (IsBusy) return;
            IsBusy = true;
            IsLoadingAddresses = true;
            IsLoadingCart = true;
            ErrorMessage = null;
            Addresses.Clear();
            SelectedAddress = null;
            Subtotal = 0;
            CalculateGrandTotal();

            try
            {
                _logger.LogInformation("Loading checkout data (Cart & Addresses).");
                // Gọi API lấy giỏ hàng và địa chỉ
                var cartTask = _cartApi.GetCart();
                var addressTask = _addressApi.GetAddresses();

                await Task.WhenAll(cartTask, addressTask);

                // Xử lý Cart Response
                var cartResponse = await cartTask;
                IsLoadingCart = false;
                if (cartResponse.IsSuccessStatusCode && cartResponse.Content != null)
                {
                    Subtotal = cartResponse.Content.Sum(item => item.Book.Price * item.Quantity);
                    _logger.LogInformation("Cart loaded with subtotal: {Subtotal}", Subtotal);
                }
                else
                {
                    string cartError = cartResponse.Error?.Content ?? cartResponse.ReasonPhrase ?? "Failed to load cart.";
                    ErrorMessage = ErrorMessage + "\n" + $"Cart Error: {cartError}";
                    _logger.LogWarning("Failed to load cart. Status: {StatusCode}", cartResponse.StatusCode);
                }

                // Xử lý Address Response
                var addressResponse = await addressTask;
                IsLoadingAddresses = false;
                if (addressResponse.IsSuccessStatusCode && addressResponse.Content != null)
                {
                    var tempAddressList = new List<SelectableAddressDto>();
                    var defaultAddress = addressResponse.Content.FirstOrDefault(a => a.IsDefault);
                    foreach (var address in addressResponse.Content.OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.UpdatedAtUtc))
                    {
                        var selectable = new SelectableAddressDto(address);
                        if (address.Id == defaultAddress?.Id)
                        {
                            selectable.IsSelected = true;
                            SelectedAddress = selectable;
                        }
                        tempAddressList.Add(selectable);
                    }
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Addresses.Clear();
                        foreach (var item in tempAddressList) Addresses.Add(item);
                        OnPropertyChanged(nameof(HasAddresses));
                        OnPropertyChanged(nameof(NoAddressesFound));
                    });
                    _logger.LogInformation("Loaded {Count} addresses.", Addresses.Count);
                }
                else
                {
                    string addrError = addressResponse.Error?.Content ?? addressResponse.ReasonPhrase ?? "Failed to load addresses.";
                    ErrorMessage = ErrorMessage + "\n" + $"Address Error: {addrError}";
                    _logger.LogWarning("Failed to load addresses. Status: {StatusCode}", addressResponse.StatusCode);
                }

                CalculateGrandTotal();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading checkout data.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                IsLoadingAddresses = false;
                IsLoadingCart = false;
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(HasAddresses));
                OnPropertyChanged(nameof(NoAddressesFound));
            }
        }

        // Command được gọi khi RadioButton hoặc TapGestureRecognizer được kích hoạt
        [RelayCommand]
        private void SelectAddress(SelectableAddressDto? selected)
        {
            if (selected == null || selected == SelectedAddress) return;

            _logger.LogInformation("Address selected: {AddressId}", selected.Address.Id);

            // Bỏ chọn cái cũ (nếu có)
            if (SelectedAddress != null)
            {
                SelectedAddress.IsSelected = false;
            }
            // Chọn cái mới
            selected.IsSelected = true;
            SelectedAddress = selected; // Cập nhật SelectedAddress
        }


        [RelayCommand]
        private async Task GoToAddAddressAsync()
        {
            _logger.LogInformation("Navigating to Add Address Page from Checkout.");
            await Shell.Current.GoToAsync($"{nameof(AddEditAddressPage)}?AddressId={Guid.Empty}");
        }


        // Command đặt hàn
        [RelayCommand(CanExecute = nameof(CanPlaceOrder))]
        private async Task PlaceOrderAsync()
        {
            if (SelectedAddress == null || IsBusy) return;

            IsPlacingOrder = true;
            ErrorMessage = null;
            _logger.LogInformation("Place Order button clicked. Selected AddressId: {AddressId}", SelectedAddress.Address.Id);

            try
            {
                // 1. Tạo Request DTO
                var requestDto = new CreateOrderRequestDto
                {
                    ShippingAddressId = SelectedAddress.Address.Id,
                    // PromotionCode = SelectedPromotionCode
                };

                // 2. Gọi API Tạo Order
                _logger.LogInformation("Calling CreateOnlineOrder API...");
                var response = await _orderApi.CreateOnlineOrder(requestDto);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var createdOrder = response.Content;
                    _logger.LogInformation("Order {OrderId} placed successfully via API. Navigating to Order History.", createdOrder.Id);
                    #if ANDROID || IOS
                    try
                    {
                        await Toast.Make("Order placed successfully!").Show();
                    }
                    catch (Exception toastEx)
                    {
                        _logger.LogWarning(toastEx, "Failed to show Toast notification.");
                    }
                    #endif
                    await Shell.Current.GoToAsync($"//{nameof(OrderHistoryPage)}");
                }
                else
                {
                    // Xử lý lỗi từ API
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to place order.";
                    ErrorMessage = $"Order Placement Failed: {errorContent}";
                    _logger.LogWarning("Failed to place order. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Order Failed", ErrorMessage);
                }
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API Exception during place order.");
                ErrorMessage = $"An API error occurred: {apiEx.Message} - {apiEx.Content}";
                await DisplayAlertAsync("API Error", ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during place order.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsPlacingOrder = false;
            }
        }

        //Helper to Calculate Grand Total
        private void CalculateGrandTotal()
        {
            GrandTotal = Math.Max(0, Subtotal + ShippingFee - Discount);
            _logger.LogInformation("Calculated GrandTotal: {GrandTotal} (Sub: {Subtotal}, Ship: {ShippingFee}, Disc: {Discount})", GrandTotal, Subtotal, ShippingFee, Discount);
        }
        public void OnAppearing()
        {
            LoadCheckoutDataCommand.Execute(null);
        }
    }
}