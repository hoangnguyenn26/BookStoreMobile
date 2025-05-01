using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminBookListViewModel : BaseViewModel
    {
        private readonly IBooksApi _booksApi;
        private readonly ICategoriesApi _categoriesApi;
        private readonly IAuthorApi _authorApi;
        private readonly ILogger<AdminBookListViewModel> _logger;

        private int _currentPage = 1;
        private const int PageSize = 15;
        private bool _isLoadingMore = false;
        private bool _canLoadMore = true;

        public AdminBookListViewModel(IBooksApi booksApi, ICategoriesApi categoriesApi, IAuthorApi authorApi, ILogger<AdminBookListViewModel> logger)
        {
            _booksApi = booksApi;
            _categoriesApi = categoriesApi;
            _authorApi = authorApi;
            _logger = logger;
            Title = "Manage Books";
            Books = new ObservableCollection<BookDto>();
            Categories = new ObservableCollection<CategoryDto>();
            Authors = new ObservableCollection<AuthorDto>();
            LoadFilterOptionsCommand.Execute(null);
        }

        [ObservableProperty] private ObservableCollection<BookDto> _books;
        [ObservableProperty] private ObservableCollection<CategoryDto> _categories;
        [ObservableProperty] private ObservableCollection<AuthorDto> _authors;
        [ObservableProperty] private string? _searchTerm;
        [ObservableProperty] private CategoryDto? _selectedCategoryFilter;
        [ObservableProperty] private AuthorDto? _selectedAuthorFilter;
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && !HasError;

        partial void OnSelectedCategoryFilterChanged(CategoryDto? value) => LoadBooksCommand.Execute(true);
        partial void OnSelectedAuthorFilterChanged(AuthorDto? value) => LoadBooksCommand.Execute(true);

        [RelayCommand]
        private async Task LoadFilterOptionsAsync()
        {
            try
            {
                var catResponse = await _categoriesApi.GetCategories();
                if (catResponse.IsSuccessStatusCode && catResponse.Content != null)
                {
                    Categories.Clear();
                    Categories.Add(new CategoryDto { Id = Guid.Empty, Name = "All Categories" });
                    foreach (var cat in catResponse.Content.OrderBy(c => c.Name)) Categories.Add(cat);
                }
                var authResponse = await _authorApi.GetAuthors();
                if (authResponse.IsSuccessStatusCode && authResponse.Content != null)
                {
                    Authors.Clear();
                    Authors.Add(new AuthorDto { Id = Guid.Empty, Name = "All Authors" });
                    foreach (var auth in authResponse.Content.OrderBy(a => a.Name)) Authors.Add(auth);
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to load filter options"); }
        }

        [RelayCommand]
        private async Task LoadBooksAsync(bool isRefreshing = false)
        {
            if (_isLoadingMore || (!isRefreshing && !_canLoadMore)) return;
            if (!isRefreshing && IsBusy) return;

            IsBusy = true;
            if (isRefreshing) { _currentPage = 1; Books.Clear(); _canLoadMore = true; }
            ErrorMessage = null;

            try
            {
                Guid? categoryFilterId = (SelectedCategoryFilter?.Id == Guid.Empty) ? null : SelectedCategoryFilter?.Id;
                Guid? authorFilterId = (SelectedAuthorFilter?.Id == Guid.Empty) ? null : SelectedAuthorFilter?.Id;

                _logger.LogInformation("Loading books for admin. Cat: {CatId}, Auth: {AuthId}, Search: {Search}, Page: {Page}",
                    categoryFilterId ?? Guid.Empty, authorFilterId ?? Guid.Empty, SearchTerm ?? "N/A", _currentPage);

                var response = await _booksApi.GetBooks(categoryFilterId, authorFilterId, SearchTerm, _currentPage, PageSize);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (response.Content.Any())
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            foreach (var book in response.Content) Books.Add(book);
                        });
                        _currentPage++;
                        _canLoadMore = response.Content.Count() == PageSize;
                    }
                    else { _canLoadMore = false; }
                    _logger.LogInformation("Loaded {Count} books. Can load more: {CanLoadMore}", response.Content?.Count() ?? 0, _canLoadMore);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load books.";
                    _logger.LogWarning("Failed to load books. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex) { ErrorMessage = ex.Message; _logger.LogError(ex, "Error loading books."); }
            finally { IsBusy = false; OnPropertyChanged(nameof(ShowContent)); }
        }

        [RelayCommand]
        private async Task LoadMoreBooksAsync()
        {
            if (_isLoadingMore || !_canLoadMore || IsBusy) return;
            _isLoadingMore = true;
            await LoadBooksAsync(isRefreshing: false);
            _isLoadingMore = false;
        }

        [RelayCommand]
        private void ClearFilters()
        {
            if (IsBusy) return;
            SelectedCategoryFilter = Categories.FirstOrDefault(c => c.Id == Guid.Empty); // Chọn "All"
            SelectedAuthorFilter = Authors.FirstOrDefault(a => a.Id == Guid.Empty); // Chọn "All"
            SearchTerm = string.Empty;
            OnPropertyChanged(nameof(SearchTerm));
        }


        [RelayCommand] private async Task SearchAsync() => await LoadBooksAsync(true);
        [RelayCommand] private async Task GoToAddBookAsync() => await Shell.Current.GoToAsync($"{nameof(AddEditBookPage)}?BookId={Guid.Empty}");
        [RelayCommand] private async Task GoToEditBookAsync(Guid? bookId) { if (bookId.HasValue) await Shell.Current.GoToAsync($"{nameof(AddEditBookPage)}?BookId={bookId.Value}"); }

        [RelayCommand]
        private async Task DeleteBookAsync(Guid? bookId)
        {
            if (!bookId.HasValue || IsBusy) return;
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Delete this book?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                var response = await _booksApi.DeleteBook(bookId.Value);
                if (response.IsSuccessStatusCode)
                {
                    await LoadBooksAsync(true); // Reload
                }
                else
                {
                    string error = response.Error?.Content ?? "Failed to delete";
                    await DisplayAlertAsync("Error", error);
                }
            }
            catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message); }
            finally { IsBusy = false; }
        }

        public void OnAppearing() { if (Books.Count == 0) LoadBooksCommand.Execute(false); }
    }
}