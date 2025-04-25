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
    [QueryProperty(nameof(BookIdQueryParam), "BookId")]
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
        private string? _bookIdQueryParam;

        private Guid _actualBookId;
        public Guid ActualBookId
        {
            get => _actualBookId;
            private set => SetProperty(ref _actualBookId, value);
        }

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


        async partial void OnBookIdQueryParamChanged(string? value)
        {
            _logger.LogInformation("BookId query parameter received: {QueryParamValue}", value ?? "NULL");
            if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out Guid parsedGuid))
            {
                ActualBookId = parsedGuid;
                Title = "Loading...";
                _logger.LogInformation("Parsed BookId: {ParsedBookId}. Loading details...", ActualBookId);
                await LoadBookDetailsAsync();
            }
            else
            {
                _logger.LogWarning("Failed to parse BookId from query parameter: {QueryParamValue}", value ?? "NULL");
                ErrorMessage = "Invalid Book Identifier received.";
                ActualBookId = Guid.Empty;
                BookDetails = null;
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        // --- Commands ---
        [RelayCommand]
        private async Task LoadBookDetailsAsync()
        {
            if (IsBusy || ActualBookId == Guid.Empty) return;
            IsBusy = true;
            ErrorMessage = null;
            BookDetails = null;
            BookDetailItems.Clear();
            IsInWishlist = false;

            try
            {
                _logger.LogInformation("Loading book details for ActualBookId: {BookId}", ActualBookId);
                var bookResponse = await _booksApi.GetBookById(ActualBookId);

                if (bookResponse.IsSuccessStatusCode && bookResponse.Content != null)
                {
                    BookDetails = bookResponse.Content;
                    Title = BookDetails.Title;
                    PrepareDetailItems();
                    if (_authService.IsLoggedIn) { await CheckWishlistStatusAsync(); }
                    _logger.LogInformation("Book details loaded successfully for ActualBookId: {BookId}.", ActualBookId);
                }
                else
                {
                    string errorContent = bookResponse.Error?.Content ?? bookResponse.ReasonPhrase ?? "Failed to load book details.";

                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load book details for ActualBookId {BookId}. Status: {StatusCode}, Reason: {Reason}", ActualBookId, bookResponse.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading book details for ActualBookId: {BookId}", ActualBookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;

                OnPropertyChanged(nameof(ShowContent));
            }
        }

        // Hàm kiểm tra trạng thái wishlist riêng
        private async Task CheckWishlistStatusAsync()
        {
            try
            {
                _logger.LogInformation("Checking wishlist status for Book {ActualBookId}", ActualBookId);
                // Gọi API lấy toàn bộ wishlist và kiểm tra (nếu không có API check riêng)
                var wishlistResponse = await _wishlistApi.GetWishlist();
                if (wishlistResponse.IsSuccessStatusCode && wishlistResponse.Content != null)
                {
                    IsInWishlist = wishlistResponse.Content.Any(item => item.BookId == ActualBookId);
                    _logger.LogInformation("Wishlist status for Book {ActualBookId}: {IsInWishlist}", ActualBookId, IsInWishlist);
                }
                else
                {
                    _logger.LogWarning("Failed to get wishlist to check status. Status: {StatusCode}", wishlistResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while checking wishlist status for Book {ActualBookId}", ActualBookId);
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
                    _logger.LogInformation("Removing book {BookId} from wishlist for User {UserId}", ActualBookId, _authService.CurrentUser?.Id);
                    response = await _wishlistApi.RemoveFromWishlist(ActualBookId);
                }
                else
                {
                    _logger.LogInformation("Adding book {BookId} to wishlist for User {UserId}", ActualBookId, _authService.CurrentUser?.Id);
                    response = await _wishlistApi.AddToWishlist(ActualBookId);
                }

                if (response.IsSuccessStatusCode)
                {
                    IsInWishlist = !IsInWishlist;
                    _logger.LogInformation("Wishlist status toggled successfully for Book {ActualBookId}. New status: {IsInWishlist}", ActualBookId, IsInWishlist);
                }
                else
                {
                    string errorContent = response.Error?.Content
                         ?? response.ReasonPhrase
                         ?? "Failed to update wishlist.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to toggle wishlist for Book {ActualBookId}. Status: {StatusCode}, Reason: {Reason}", ActualBookId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while toggling wishlist for Book {ActualBookId}", ActualBookId);
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
            _logger.LogInformation("Add to cart clicked for Book {ActualBookId}", ActualBookId);
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