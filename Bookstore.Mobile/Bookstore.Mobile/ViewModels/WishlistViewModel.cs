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

        public WishlistViewModel(IWishlistApi wishlistApi, IAuthService authService, ILogger<WishlistViewModel> logger)
        {
            _wishlistApi = wishlistApi ?? throw new ArgumentNullException(nameof(wishlistApi));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Title = "My Wishlist";
            WishlistItems = new ObservableCollection<WishlistItemDto>();
        }

        [ObservableProperty]
        private ObservableCollection<WishlistItemDto> _wishlistItems;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _showContent;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        [RelayCommand]
        private async Task LoadWishlistAsync()
        {
            if (IsBusy) return;

            if (!_authService.IsLoggedIn)
            {
                _logger.LogWarning("User not logged in. Cannot load wishlist.");
                ErrorMessage = "Please login to view your wishlist.";
                WishlistItems.Clear();
                ShowContent = !IsBusy && !HasError;
                return;
            }

            IsBusy = true;
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Loading user wishlist...");
                var response = await _wishlistApi.GetWishlist();

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    WishlistItems.Clear();
                    foreach (var item in response.Content.OrderByDescending(i => i.CreatedAtUtc))
                    {
                        if (item.Book != null)
                        {
                            WishlistItems.Add(item);
                        }
                        else
                        {
                            _logger.LogWarning("Wishlist item with BookId {BookId} has null Book details. Skipping.", item.BookId);
                        }
                    }
                    _logger.LogInformation("Loaded {Count} items in wishlist.", WishlistItems.Count);
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to load wishlist.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load wishlist. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading wishlist.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                ShowContent = !IsBusy && !HasError;
            }
        }

        [RelayCommand]
        private async Task RemoveFromWishlistAsync(Guid? bookId)
        {
            if (!bookId.HasValue || bookId.Value == Guid.Empty || IsBusy) return;

            var itemToRemove = WishlistItems.FirstOrDefault(item => item.Book.Id == bookId.Value);
            if (itemToRemove == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Remove Item", $"Remove '{itemToRemove.Book.Title}' from your wishlist?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Removing Book {BookId} from wishlist.", bookId.Value);

            try
            {
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
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to remove Book {BookId} from wishlist. Status: {StatusCode}, Reason: {Reason}", bookId.Value, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while removing Book {BookId} from wishlist.", bookId.Value);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
                ShowContent = !IsBusy && !HasError;
            }
        }

        [RelayCommand]
        private async Task GoToBookDetailsAsync(Guid? bookId)
        {
            if (!bookId.HasValue || bookId.Value == Guid.Empty) return;
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
            LoadWishlistCommand.Execute(null);
        }
    }
}