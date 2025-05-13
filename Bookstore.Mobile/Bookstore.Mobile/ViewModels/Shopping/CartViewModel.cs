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
            // Subscribe to collection changes to update dependent properties
            CartItems.CollectionChanged += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(HasItems));
                    UpdateSubtotal();
                });
            };
        }

        [ObservableProperty]
        private ObservableCollection<CartItemDto> _cartItems;

        [ObservableProperty]
        private decimal _subtotal;

        // Make this observable
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowContent))]
        private bool _hasItems;

        // Override this property from BaseViewModel to include HasItems
        public override bool ShowContent => !IsBusy && !HasError && HasItems;

        private void UpdateSubtotal()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Subtotal = CartItems?.Sum(item => item.TotalItemPrice) ?? 0m;
                // Not using the property but directly checking
                bool hasAnyItems = CartItems?.Count > 0;
                // Only update if different to avoid unnecessary UI updates
                if (hasAnyItems != _hasItems)
                {
                    HasItems = hasAnyItems;
                }
                OnPropertyChanged(nameof(ShowContent));
            });
        }

        [RelayCommand]
        private async Task LoadCartAsync()
        {
            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Loading cart items for user {UserId}", _authService.CurrentUser?.Id);
                var response = await _cartApi.GetCart();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        CartItems.Clear();
                        foreach (var item in response.Content)
                        {
                            CartItems.Add(item);
                        }
                        UpdateSubtotal();
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = response.Error?.Content ?? "Failed to load cart items.";
                    });
                }
            }, true, nameof(ShowContent));
        }

        [RelayCommand]
        private async Task IncreaseQuantityAsync(CartItemDto item)
        {
            await RunSafeAsync(async () =>
            {
                var updateDto = new UpdateCartItemDto { Quantity = item.Quantity + 1 };
                var response = await _cartApi.UpdateItemQuantity(item.BookId, updateDto);
                if (response.IsSuccessStatusCode)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        item.Quantity++;
                        UpdateSubtotal();
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = response.Error?.Content ?? "Failed to update quantity.";
                    });
                }
            }, true, nameof(ShowContent));
        }

        [RelayCommand]
        private async Task DecreaseQuantityAsync(CartItemDto item)
        {
            await RunSafeAsync(async () =>
            {
                if (item.Quantity <= 1) return;
                var updateDto = new UpdateCartItemDto { Quantity = item.Quantity - 1 };
                var response = await _cartApi.UpdateItemQuantity(item.BookId, updateDto);
                if (response.IsSuccessStatusCode)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        item.Quantity--;
                        UpdateSubtotal();
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = response.Error?.Content ?? "Failed to update quantity.";
                    });
                }
            }, true, nameof(ShowContent));
        }

        [RelayCommand]
        private async Task RemoveItemAsync(CartItemDto item)
        {
            await RunSafeAsync(async () =>
            {
                var response = await _cartApi.RemoveItem(item.BookId);
                if (response.IsSuccessStatusCode)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        CartItems.Remove(item);
                        UpdateSubtotal();
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = response.Error?.Content ?? "Failed to remove item.";
                    });
                }
            }, true, nameof(ShowContent));
        }

        [RelayCommand]
        private async Task ClearCartAsync()
        {
            await RunSafeAsync(async () =>
            {
                var response = await _cartApi.ClearCart();
                if (response.IsSuccessStatusCode)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        CartItems.Clear();
                        UpdateSubtotal();
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = response.Error?.Content ?? "Failed to clear cart.";
                    });
                }
            }, true, nameof(ShowContent));
        }

        // Command đi đến trang Checkout
        [RelayCommand]
        private async Task GoToCheckoutAsync()
        {
            if (IsBusy || !CartItems.Any()) return;
            _logger.LogInformation("Navigating to Checkout Page.");
            await Shell.Current.GoToAsync(nameof(CheckoutPage));
            //await _navigationService.NavigateToAsync(nameof(CheckoutPage));
        }

        // Command quay lại mua sắm (khi giỏ hàng trống)
        [RelayCommand]
        private async Task GoShoppingAsync()
        {
            _logger.LogInformation("Navigating to Home Page from empty Cart.");
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        public void OnAppearing()
        {
            LoadCartCommand.Execute(null);
        }
    }
}