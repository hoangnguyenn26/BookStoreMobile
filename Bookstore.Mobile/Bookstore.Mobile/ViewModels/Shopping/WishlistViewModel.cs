using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class WishlistViewModel : BaseViewModel
    {
        private readonly IWishlistApi _wishlistApi;
        private readonly IAuthService _authService;
        private readonly ILogger<WishlistViewModel> _logger;

        public WishlistViewModel(IWishlistApi wishlistApi,
                               IAuthService authService,
                               ILogger<WishlistViewModel> logger)
        {
            _wishlistApi = wishlistApi ?? throw new ArgumentNullException(nameof(wishlistApi));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Title = "My Wishlist";
            WishlistItems = new ObservableCollection<WishlistItemDto>();
        }

        [ObservableProperty]
        private ObservableCollection<WishlistItemDto> _wishlistItems;

        [RelayCommand]
        private async Task LoadWishlistAsync()
        {
            await RunSafeAsync(async () =>
            {
                if (!_authService.IsLoggedIn)
                {
                    _logger.LogWarning("User not logged in. Cannot load wishlist.");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = "Please login to view your wishlist.";
                        WishlistItems.Clear();
                    });
                    return;
                }

                var response = await _wishlistApi.GetWishlist();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        WishlistItems.Clear();
                        foreach (var item in response.Content)
                        {
                            WishlistItems.Add(item);
                        }
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = response.Error?.Content ?? "Failed to load wishlist.";
                        WishlistItems.Clear();
                    });
                }
            }, true);
        }

        [RelayCommand]
        private async Task RemoveFromWishlistAsync(Guid? bookId)
        {
            if (!bookId.HasValue || bookId.Value == Guid.Empty || IsBusy) return;

            var itemToRemove = WishlistItems.FirstOrDefault(item => item.Book.Id == bookId.Value);
            if (itemToRemove == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Remove Item",
                $"Remove '{itemToRemove.Book.Title}' from your wishlist?",
                "Yes",
                "No");
            if (!confirm) return;

            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Removing Book {BookId} from wishlist.", bookId.Value);
                var response = await _wishlistApi.RemoveFromWishlist(bookId.Value);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Book {BookId} removed successfully from wishlist.", bookId.Value);
                    var itemInCollection = WishlistItems.FirstOrDefault(item => item.Book.Id == bookId.Value);
                    if (itemInCollection != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => WishlistItems.Remove(itemInCollection));
                    }
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to remove item.";
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = $"Error: {errorContent}";
                    });
                    _logger.LogWarning("Failed to remove Book {BookId} from wishlist. Status: {StatusCode}, Reason: {Reason}",
                        bookId.Value, response.StatusCode, ErrorMessage);
                }
            }, true);
        }

        [RelayCommand]
        private async Task GoToBookDetailsAsync(Guid? bookId)
        {
            if (!bookId.HasValue || bookId.Value == Guid.Empty)
            {
                _logger.LogWarning("Attempted to navigate to book details with invalid book ID");
                return;
            }

            _logger.LogInformation("Navigating to Book Details for Id: {BookId} from wishlist.", bookId.Value);
            await Shell.Current.GoToAsync($"{nameof(BookDetailsPage)}?BookId={bookId.Value}");
        }

        [RelayCommand]
        private async Task GoShoppingAsync()
        {
            _logger.LogInformation("Navigating to Home Page from empty wishlist.");
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        public void OnAppearing()
        {
            if (!IsBusy)
            {
                LoadWishlistCommand.Execute(null);
            }
        }
    }
}