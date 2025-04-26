using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class CartViewModel : BaseViewModel
    {
        private readonly ICartApi _cartApi;
        private readonly IAuthService _authService;
        private readonly ILogger<CartViewModel> _logger;
        // private readonly INavigationService _navigationService;

        public CartViewModel(ICartApi cartApi, IAuthService authService, ILogger<CartViewModel> logger /*, INavigationService navigationService*/)
        {
            _cartApi = cartApi;
            _authService = authService;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "My Cart";
            CartItems = new ObservableCollection<CartItemDto>();
        }

        [ObservableProperty]
        private ObservableCollection<CartItemDto> _cartItems;

        [ObservableProperty]
        private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        [ObservableProperty]
        private decimal _subtotal;

        public bool HasItems => CartItems.Count > 0;
        public bool ShowContent => !IsBusy && !HasError;

        // --- Commands ---
        [RelayCommand]
        private async Task LoadCartAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Loading cart items for user {UserId}", _authService.CurrentUser?.Id);
                var response = await _cartApi.GetCart();

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    CartItems.Clear();
                    foreach (var item in response.Content)
                    {
                        CartItems.Add(item);
                    }
                    CalculateSubtotal();
                    _logger.LogInformation("Loaded {Count} items in cart.", CartItems.Count);
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to load cart.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load cart. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading cart.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(HasItems));
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        // Tính tổng tiền
        private void CalculateSubtotal()
        {
            Subtotal = CartItems.Sum(item => item.TotalItemPrice);
        }

        // Command tăng số lượng (Sẽ gọi API ở ngày mai)
        [RelayCommand]
        private async Task IncreaseQuantityAsync(CartItemDto? item)
        {
            if (item == null || IsBusy) return;
            _logger.LogInformation("IncreaseQuantityCommand for BookId {BookId}", item.BookId);
            //Gọi API PUT /api/v1/cart/items/{bookId} để cập nhật số lượng (+1)
            // Tạm thời tăng số lượng trên UI và tính lại tổng tiền
            item.Quantity++;
            OnPropertyChanged(nameof(item.Quantity));
            OnPropertyChanged(nameof(item.TotalItemPrice));
            CalculateSubtotal();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task DecreaseQuantityAsync(CartItemDto? item)
        {
            if (item == null || IsBusy || item.Quantity <= 1) return;
            _logger.LogInformation("DecreaseQuantityCommand for BookId {BookId}", item.BookId);
            // Gọi API PUT /api/v1/cart/items/{bookId} để cập nhật số lượng (-1)
            // Tạm thời giảm số lượng trên UI
            item.Quantity--;
            OnPropertyChanged(nameof(item.Quantity));
            OnPropertyChanged(nameof(item.TotalItemPrice));
            CalculateSubtotal();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task RemoveItemAsync(CartItemDto? item)
        {
            if (item == null || IsBusy) return;
            _logger.LogInformation("RemoveItemCommand for BookId {BookId}", item.BookId);
            //  Gọi API DELETE /api/v1/cart/items/{bookId}
            // Tạm thời xóa khỏi ObservableCollection
            CartItems.Remove(item);
            CalculateSubtotal();
            OnPropertyChanged(nameof(HasItems));
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ClearCartAsync()
        {
            if (IsBusy || !CartItems.Any()) return;
            _logger.LogInformation("ClearCartCommand executed.");
            // Gọi API DELETE /api/v1/cart
            // Tạm thời xóa hết trên UI
            CartItems.Clear();
            CalculateSubtotal();
            OnPropertyChanged(nameof(HasItems));
            await Task.CompletedTask;
        }

        // Command đi đến trang Checkout
        [RelayCommand]
        private async Task GoToCheckoutAsync()
        {
            if (IsBusy || !CartItems.Any()) return;
            _logger.LogInformation("Navigating to Checkout Page.");
            //await Shell.Current.GoToAsync(nameof(CheckoutPage));
            // await _navigationService.NavigateToAsync(nameof(CheckoutPage));
        }

        // Command quay lại mua sắm (khi giỏ hàng trống)
        [RelayCommand]
        private async Task GoShoppingAsync()
        {
            _logger.LogInformation("Navigating back to shop (e.g., Home or Categories).");
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        public void OnAppearing()
        {
            LoadCartCommand.Execute(null);
        }
    }
}