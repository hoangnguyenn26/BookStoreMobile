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

            IsBusy = true; // Bắt đầu xử lý
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Increasing quantity for BookId {BookId}", item.BookId);
                var updateDto = new UpdateCartItemDto { Quantity = item.Quantity + 1 };

                var response = await _cartApi.UpdateItemQuantity(item.BookId, updateDto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Quantity increased successfully for BookId {BookId}", item.BookId);
                    item.Quantity++;
                    CalculateSubtotal();
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to update quantity.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to increase quantity for BookId {BookId}. Status: {StatusCode}, Reason: {Reason}", item.BookId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", $"Could not update quantity for {item.Book.Title}: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while increasing quantity for BookId {BookId}", item.BookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DecreaseQuantityAsync(CartItemDto? item)
        {
            if (item == null || IsBusy || item.Quantity <= 1) return;

            IsBusy = true;
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Decreasing quantity for BookId {BookId}", item.BookId);
                var updateDto = new UpdateCartItemDto { Quantity = item.Quantity - 1 };

                var response = await _cartApi.UpdateItemQuantity(item.BookId, updateDto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Quantity decreased successfully for BookId {BookId}", item.BookId);
                    // Cập nhật UI
                    item.Quantity--;
                    OnPropertyChanged(nameof(item.Quantity));
                    OnPropertyChanged(nameof(item.TotalItemPrice));
                    CalculateSubtotal();
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to update quantity.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to decrease quantity for BookId {BookId}. Status: {StatusCode}, Reason: {Reason}", item.BookId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", $"Could not update quantity for {item.Book.Title}: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while decreasing quantity for BookId {BookId}", item.BookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RemoveItemAsync(CartItemDto? item)
        {
            if (item == null || IsBusy) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", $"Remove '{item.Book.Title}' from cart?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Removing item with BookId {BookId} from cart", item.BookId);
                var response = await _cartApi.RemoveItem(item.BookId);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Item with BookId {BookId} removed successfully.", item.BookId);
                    CartItems.Remove(item);
                    CalculateSubtotal();
                    OnPropertyChanged(nameof(HasItems));
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to remove item.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to remove item with BookId {BookId}. Status: {StatusCode}, Reason: {Reason}", item.BookId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", $"Could not remove '{item.Book.Title}' from cart: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while removing item with BookId {BookId}", item.BookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ClearCartAsync()
        {
            if (IsBusy || !CartItems.Any()) return;

            // Hỏi xác nhận
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Clear Cart", "Are you sure you want to remove all items from your cart?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Clearing user cart.");
                var response = await _cartApi.ClearCart();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Cart cleared successfully.");
                    // Xóa hết trên UI
                    CartItems.Clear();
                    CalculateSubtotal();
                    OnPropertyChanged(nameof(HasItems));
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to clear cart.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to clear cart. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", $"Could not clear cart: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while clearing cart.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
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