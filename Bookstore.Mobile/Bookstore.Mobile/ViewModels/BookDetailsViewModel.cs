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
    [QueryProperty(nameof(BookIdString), "BookId")]
    public partial class BookDetailsViewModel : BaseViewModel
    {
        private readonly IBooksApi _booksApi;
        private readonly IWishlistApi _wishlistApi;
        private readonly ICartApi _cartApi;
        private readonly IReviewApi _reviewApi;
        private readonly IAuthService _authService;
        private readonly ILogger<BookDetailsViewModel> _logger;

        public BookDetailsViewModel(IBooksApi booksApi, IWishlistApi wishlistApi, ICartApi cartApi, IReviewApi reviewApi, IAuthService authService, ILogger<BookDetailsViewModel> logger)
        {
            _booksApi = booksApi;
            _wishlistApi = wishlistApi;
            _cartApi = cartApi;
            _reviewApi = reviewApi; // Gán giá trị
            _authService = authService;
            _logger = logger;
            Title = "Book Details";
            BookDetailItems = new ObservableCollection<KeyValuePair<string, string>>();
            Reviews = new ObservableCollection<ReviewDto>(); // Khởi tạo
            QuantityToAdd = 1; // Mặc định
        }
        private Guid _actualBookId = Guid.Empty;
        private string? _bookIdString;
        public string? BookIdString
        {
            get => _bookIdString;
            set
            {
                if (_bookIdString != value)
                {
                    _bookIdString = value;
                    // Gọi hàm xử lý Id dạng string
                    ProcessBookId(value);
                }
            }
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

        [ObservableProperty]
        private ObservableCollection<ReviewDto> _reviews;

        [ObservableProperty]
        private bool _isLoadingReviews;

        [ObservableProperty]
        private string? _reviewsErrorMessage;

        public bool HasReviewsError => !string.IsNullOrEmpty(ReviewsErrorMessage);
        public bool ShowReviewsContent => !IsLoadingReviews && !HasReviewsError;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddToCartCommand))]
        private int _quantityToAdd = 1;

        public bool CanAddToCart => BookDetails != null && BookDetails.StockQuantity > 0 && QuantityToAdd > 0 && QuantityToAdd <= BookDetails.StockQuantity && IsNotBusy;

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<string, string>> _bookDetailItems;

        [RelayCommand]
        private async Task LoadBookDetailsAsync()
        {
            if (IsBusy || _actualBookId == Guid.Empty) return;
            IsBusy = true;
            ErrorMessage = null;
            ReviewsErrorMessage = null; // Reset lỗi review
            BookDetails = null;
            BookDetailItems.Clear();
            Reviews.Clear(); // Xóa review cũ
            IsInWishlist = false;
            QuantityToAdd = 1; // Reset số lượng về 1

            try
            {
                _logger.LogInformation("Loading book details for Id: {BookId}", _actualBookId);
                var bookResponse = await _booksApi.GetBookById(_actualBookId);

                if (bookResponse.IsSuccessStatusCode && bookResponse.Content != null)
                {
                    BookDetails = bookResponse.Content;
                    Title = BookDetails.Title;
                    PrepareDetailItems();
                    _logger.LogInformation("Book details loaded successfully.");

                    await LoadReviewsAsync();

                    if (_authService.IsLoggedIn)
                    {
                        await CheckWishlistStatusAsync();
                    }
                }
                else
                {
                    string errorContent = bookResponse.Error?.Content ?? bookResponse.ReasonPhrase ?? "Failed to load book details.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load book details for Id {BookId}. Status: {StatusCode}, Reason: {Reason}", _actualBookId, bookResponse.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading book details for Id: {BookId}", _actualBookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(CanAddToCart));
            }
        }

        private async Task LoadReviewsAsync()
        {
            if (BookDetails == null) return;
            IsLoadingReviews = true;
            ReviewsErrorMessage = null;
            Reviews.Clear();
            try
            {
                _logger.LogInformation("Loading reviews for Book {BookId}", _actualBookId);
                var reviewResponse = await _reviewApi.GetBookReviews(_actualBookId, 1, 5);

                if (reviewResponse.IsSuccessStatusCode && reviewResponse.Content != null)
                {
                    foreach (var review in reviewResponse.Content) { Reviews.Add(review); }
                    _logger.LogInformation("Loaded {Count} reviews.", Reviews.Count);
                }
                else
                {
                    string errorContent = reviewResponse.Error?.Content ?? reviewResponse.ReasonPhrase ?? "Failed to load reviews.";
                    ReviewsErrorMessage = $"Error loading reviews: {errorContent}";
                    _logger.LogWarning("Failed to load reviews for Book {BookId}. Status: {StatusCode}", _actualBookId, reviewResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading reviews for Book {BookId}", _actualBookId);
                ReviewsErrorMessage = "An error occurred while loading reviews.";
            }
            finally
            {
                IsLoadingReviews = false;
                OnPropertyChanged(nameof(ShowReviewsContent));
            }
        }


        private async Task CheckWishlistStatusAsync()
        {
            if (!_authService.IsLoggedIn || BookDetails == null)
            {
                IsInWishlist = false; // Không đăng nhập thì không có wishlist
                return;
            }
            try
            {
                _logger.LogInformation("Checking wishlist status for Book {BookId}", BookDetails.Id);
                var wishlistResponse = await _wishlistApi.GetWishlist();

                if (wishlistResponse.IsSuccessStatusCode && wishlistResponse.Content != null)
                {
                    IsInWishlist = wishlistResponse.Content.Any(item => item.BookId == BookDetails.Id);
                    _logger.LogInformation("Wishlist status for Book {BookId}: {IsInWishlist}", BookDetails.Id, IsInWishlist);
                }
                else
                {
                    _logger.LogWarning("Failed to get wishlist to check status. Status: {StatusCode}", wishlistResponse.StatusCode);
                    IsInWishlist = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while checking wishlist status for Book {BookId}", BookDetails.Id);
                IsInWishlist = false;
            }
            finally
            {
            }
        }

        private bool CanToggleWishlist() => BookDetails != null && IsNotBusy;
        [RelayCommand(CanExecute = nameof(CanToggleWishlist))]
        private async Task ToggleWishlistAsync()
        {
            if (!_authService.IsLoggedIn)
            {
                await DisplayAlertAsync("Login Required", "Please login to manage your wishlist.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                return;
            }

            if (BookDetails == null || IsBusy) return;

            IsBusy = true;
            ErrorMessage = null;

            try
            {
                ApiResponse<object>? response;
                if (IsInWishlist)
                {
                    _logger.LogInformation("Removing book {BookId} from wishlist.", BookDetails.Id);
                    response = await _wishlistApi.RemoveFromWishlist(BookDetails.Id);
                }
                else
                {
                    _logger.LogInformation("Adding book {BookId} to wishlist.", BookDetails.Id);
                    response = await _wishlistApi.AddToWishlist(BookDetails.Id);
                }

                if (response.IsSuccessStatusCode)
                {
                    IsInWishlist = !IsInWishlist;
                    _logger.LogInformation("Wishlist status toggled successfully for Book {BookId}. New status: {IsInWishlist}", BookDetails.Id, IsInWishlist);
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to update wishlist.";
                    ErrorMessage = $"Wishlist Error: {errorContent}";
                    _logger.LogWarning("Failed to toggle wishlist for Book {BookId}. Status: {StatusCode}, Reason: {Reason}", BookDetails.Id, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Wishlist Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while toggling wishlist for Book {BookId}", BookDetails.Id);
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
            if (BookDetails == null || QuantityToAdd <= 0 || IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Adding {Quantity} of Book {BookId} to cart.", QuantityToAdd, _actualBookId);
                var addItemDto = new AddCartItemDto { BookId = _actualBookId, Quantity = QuantityToAdd };
                var response = await _cartApi.AddOrUpdateItem(addItemDto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Book {BookId} added/updated in cart successfully.", _actualBookId);
                    await DisplayAlertAsync("Success", $"Added {QuantityToAdd} x '{BookDetails.Title}' to cart.", "OK");
                    // QuantityToAdd = 1; // Reset lại nếu muốn
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to add to cart.";
                    ErrorMessage = errorContent.Contains("Insufficient stock") ? errorContent : $"Error: {errorContent}";
                    _logger.LogWarning("Failed to add Book {BookId} to cart. Status: {StatusCode}, Reason: {Reason}", _actualBookId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while adding Book {BookId} to cart.", _actualBookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task GoToWriteReviewAsync()
        {
            if (BookDetails == null || !_authService.IsLoggedIn)
            {
                // Có thể yêu cầu đăng nhập
                await DisplayAlertAsync("Login Required", "Please login to write a review.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                return;
            }
            _logger.LogInformation("Navigating to Submit Review Page for Book {BookId}", _actualBookId);
            // Điều hướng đến trang Submit Review
            await Shell.Current.GoToAsync($"{nameof(SubmitReviewPage)}?BookId={_actualBookId}&BookTitle={Uri.EscapeDataString(BookDetails.Title)}");
        }


        private void PrepareDetailItems()
        {
            BookDetailItems.Clear();
            if (BookDetails == null) return;
            if (!string.IsNullOrWhiteSpace(BookDetails.ISBN)) BookDetailItems.Add(new KeyValuePair<string, string>("ISBN:", BookDetails.ISBN));
            if (!string.IsNullOrWhiteSpace(BookDetails.Publisher)) BookDetailItems.Add(new KeyValuePair<string, string>("Publisher:", BookDetails.Publisher));
            if (BookDetails.PublicationYear.HasValue) BookDetailItems.Add(new KeyValuePair<string, string>("Year:", BookDetails.PublicationYear.Value.ToString()));
            if (BookDetails.Category != null) BookDetailItems.Add(new KeyValuePair<string, string>("Category:", BookDetails.Category.Name)); // Cần Include Category từ API
        }

        // Hàm xử lý chuỗi Id nhận được
        private async void ProcessBookId(string? idString)
        {
            _logger.LogInformation("Received BookId string parameter: {BookIdString}", idString);

            if (!string.IsNullOrEmpty(idString) && Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
            {
                _actualBookId = parsedId;
                await LoadBookDetailsAsync();
            }
            else
            {
                _logger.LogWarning("Invalid or empty BookId received from navigation: {BookIdString}", idString);
                ErrorMessage = "Invalid Book specified.";
                _actualBookId = Guid.Empty;
                BookDetails = null;
                Reviews.Clear();
                BookDetailItems.Clear();
            }
        }
    }
}