using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(BookId), "BookId")]
    public partial class BookDetailsViewModel : BaseViewModel
    {
        private readonly IBooksApi _booksApi;
        private readonly IWishlistApi _wishlistApi;
        private readonly IAuthService _authService;
        private readonly ILogger<BookDetailsViewModel> _logger;
        // private readonly ICartService _cartService; // Sẽ inject sau

        public BookDetailsViewModel(IBooksApi booksApi, IWishlistApi wishlistApi, IAuthService authService, ILogger<BookDetailsViewModel> logger)
        {
            _booksApi = booksApi;
            _wishlistApi = wishlistApi;
            _authService = authService;
            _logger = logger;
            Title = "Book Details";
            BookDetailItems = new ObservableCollection<KeyValuePair<string, string>>();
        }

        [ObservableProperty]
        private Guid _bookId;

        [ObservableProperty]
        private BookDto? _bookDetails;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanAddToCart))]
        private bool _isInWishlist;

        [ObservableProperty]
        private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool ShowContent => !IsBusy && BookDetails != null && !HasError;

        public bool CanAddToCart => BookDetails != null && BookDetails.StockQuantity > 0 && IsNotBusy;

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<string, string>> _bookDetailItems;


        async partial void OnBookIdChanged(Guid value)
        {
            if (value != Guid.Empty)
            {
                await LoadBookDetailsAsync();
            }
        }

        // --- Commands ---
        [RelayCommand]
        private async Task LoadBookDetailsAsync()
        {
            if (IsBusy || BookId == Guid.Empty) return;
            IsBusy = true;
            ErrorMessage = null;
            BookDetails = null;
            BookDetailItems.Clear();
            IsInWishlist = false;

            try
            {
                _logger.LogInformation("Loading book details for Id: {BookId}", BookId);

                // Gọi API lấy chi tiết sách
                var bookResponse = await _booksApi.GetBookById(BookId);

                if (bookResponse.IsSuccessStatusCode && bookResponse.Content != null)
                {
                    BookDetails = bookResponse.Content;
                    Title = BookDetails.Title;

                    PrepareDetailItems();

                    if (_authService.IsLoggedIn)
                    {
                        await CheckWishlistStatusAsync();
                    }
                    _logger.LogInformation("Book details loaded successfully.");
                }
                else
                {
                    string errorContent = bookResponse.Error?.Content ?? bookResponse.ReasonPhrase ?? "Failed to load book details.";

                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load book details for Id {BookId}. Status: {StatusCode}, Reason: {Reason}", BookId, bookResponse.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading book details for Id: {BookId}", BookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                // Thông báo thay đổi cho các thuộc tính tính toán
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        // Hàm kiểm tra trạng thái wishlist riêng
        private async Task CheckWishlistStatusAsync()
        {
            try
            {
                _logger.LogInformation("Checking wishlist status for Book {BookId}", BookId);
                // Gọi API lấy toàn bộ wishlist và kiểm tra (nếu không có API check riêng)
                var wishlistResponse = await _wishlistApi.GetWishlist();
                if (wishlistResponse.IsSuccessStatusCode && wishlistResponse.Content != null)
                {
                    IsInWishlist = wishlistResponse.Content.Any(item => item.BookId == BookId);
                    _logger.LogInformation("Wishlist status for Book {BookId}: {IsInWishlist}", BookId, IsInWishlist);
                }
                else
                {
                    _logger.LogWarning("Failed to get wishlist to check status. Status: {StatusCode}", wishlistResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while checking wishlist status for Book {BookId}", BookId);
                // Mặc định là false nếu có lỗi
                IsInWishlist = false;
            }
        }


        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task ToggleWishlistAsync()
        {
            if (!_authService.IsLoggedIn)
            {
                // Yêu cầu đăng nhập
                await DisplayAlertAsync("Login Required", "Please login to manage your wishlist.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                return;
            }

            if (BookDetails == null) return;

            IsBusy = true;
            ErrorMessage = null;
            try
            {
                ApiResponse<object>? response;
                if (IsInWishlist)
                {
                    _logger.LogInformation("Removing book {BookId} from wishlist for User {UserId}", BookId, _authService.CurrentUser?.Id);
                    response = await _wishlistApi.RemoveFromWishlist(BookId);
                }
                else
                {
                    _logger.LogInformation("Adding book {BookId} to wishlist for User {UserId}", BookId, _authService.CurrentUser?.Id);
                    response = await _wishlistApi.AddToWishlist(BookId);
                }

                if (response.IsSuccessStatusCode)
                {
                    IsInWishlist = !IsInWishlist;
                    _logger.LogInformation("Wishlist status toggled successfully for Book {BookId}. New status: {IsInWishlist}", BookId, IsInWishlist);
                }
                else
                {
                    string errorContent = response.Error?.Content
                         ?? response.ReasonPhrase
                         ?? "Failed to update wishlist.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to toggle wishlist for Book {BookId}. Status: {StatusCode}, Reason: {Reason}", BookId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while toggling wishlist for Book {BookId}", BookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }

        }

        [RelayCommand(CanExecute = nameof(CanAddToCart))]
        private async Task AddToCartAsync()
        {
            if (BookDetails == null) return;
            _logger.LogInformation("Add to cart clicked for Book {BookId}", BookId);
            //Implement Cart Logic
            int quantityToAdd = 1;
            // bool success = await _cartService.AddItemAsync(BookId, quantityToAdd);
            await DisplayAlertAsync("Add to Cart", $"Logic for adding {quantityToAdd} of '{BookDetails.Title}' to cart - To be implemented.", "OK");
        }


        // Hàm helper để chuẩn bị danh sách chi tiết khác
        private void PrepareDetailItems()
        {
            BookDetailItems.Clear();
            if (BookDetails == null) return;

            if (!string.IsNullOrWhiteSpace(BookDetails.ISBN))
                BookDetailItems.Add(new KeyValuePair<string, string>("ISBN:", BookDetails.ISBN));
            if (!string.IsNullOrWhiteSpace(BookDetails.Publisher))
                BookDetailItems.Add(new KeyValuePair<string, string>("Publisher:", BookDetails.Publisher));
            if (BookDetails.PublicationYear.HasValue)
                BookDetailItems.Add(new KeyValuePair<string, string>("Year:", BookDetails.PublicationYear.Value.ToString()));
        }
    }
}