// Bookstore.Mobile/ViewModels/BooksViewModel.cs
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(CategoryIdQuery), "CategoryId")]
    public partial class BooksViewModel : BaseViewModel
    {
        private readonly IBooksApi _booksApi;
        private readonly ILogger<BooksViewModel> _logger;
        // private readonly INavigationService _navigationService;

        private int _currentPage = 1;
        private const int PageSize = 20;
        private bool _isLoadingMore = false;
        private bool _canLoadMore = true;

        public BooksViewModel(IBooksApi booksApi, ILogger<BooksViewModel> logger /*, INavigationService navigationService*/)
        {
            _booksApi = booksApi;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "Books"; // Sẽ cập nhật sau khi có CategoryId
            Books = new ObservableCollection<BookDto>();
        }

        [ObservableProperty]
        private ObservableCollection<BookDto> _books;

        [ObservableProperty]
        private Guid? _categoryId;
        private string? _categoryIdQuery;
        public string? CategoryIdQuery
        {
            get => _categoryIdQuery;
            set
            {
                _categoryIdQuery = value;

                if (Guid.TryParse(value, out Guid parsedGuid))
                {
                    CategoryId = parsedGuid;
                }
                else
                {
                    CategoryId = null;
                    _logger.LogWarning("Could not parse CategoryId from query string: {QueryValue}", value);
                }
            }
        }
        [ObservableProperty]
        private string? _searchTerm;

        partial void OnCategoryIdChanged(Guid? value)
        {
            _logger.LogInformation("CategoryId received: {CategoryId}", value ?? Guid.Empty);
            Title = $"Category: {value}";
            Books.Clear();
            _currentPage = 1;
            _canLoadMore = true;

            if (LoadBooksCommand.CanExecute(null))
            {
                LoadBooksCommand.Execute(null);
            }
        }


        // --- Commands ---
        [RelayCommand]
        private async Task LoadBooksAsync()
        {
            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Loading books (Page: {Page})", _currentPage);
                var response = await _booksApi.GetBooks(null, null, null, _currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (response.Content.Any())
                    {
                        foreach (var book in response.Content)
                        {
                            Books.Add(book);
                        }
                        _currentPage++;
                        _canLoadMore = response.Content.Count() == PageSize;
                        _logger.LogInformation("Loaded {Count} books. Can load more: {CanLoadMore}", response.Content.Count(), _canLoadMore);
                    }
                    else
                    {
                        _canLoadMore = false;
                        _logger.LogInformation("No more books found.");
                    }
                }
                else
                {
                    string errorContent = response.Error?.Content
                         ?? response.ReasonPhrase
                         ?? "Failed to load dashboard Books.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load books. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                }
            }, nameof(ShowContent));
        }

        // Command cho Infinite Scrolling / Pull-to-refresh
        [RelayCommand]
        private async Task LoadMoreBooksAsync()
        {
            if (_isLoadingMore || !_canLoadMore) return;
            _isLoadingMore = true;
            _logger.LogInformation("LoadMoreBooksCommand triggered.");
            await LoadBooksAsync();
            _isLoadingMore = false;
        }


        // Command điều hướng đến chi tiết sách
        [RelayCommand]
        private async Task GoToBookDetailsAsync(Guid? bookId)
        {
            if (!bookId.HasValue || bookId.Value == Guid.Empty) return;
            _logger.LogInformation("Navigating to Book Details for Id: {BookId}", bookId.Value);
            await Shell.Current.GoToAsync($"{nameof(BookDetailsPage)}?BookId={bookId.Value}");
            // await _navigationService.NavigateToAsync(nameof(BookDetailsPage), new Dictionary<string, object> { { "BookId", bookId.Value } });
        }

        public void OnAppearing()
        {
            // Có thể gọi LoadBooksCommand ở đây nếu CategoryId không tự trigger OnCategoryIdChanged lần đầu
            if (Books.Count == 0 && CategoryId.HasValue && LoadBooksCommand.CanExecute(null))
            {
                LoadBooksCommand.Execute(null);
            }
        }
    }
}