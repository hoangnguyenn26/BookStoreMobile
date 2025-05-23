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
        private bool _hasLoaded = false;

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

        partial void OnSelectedCategoryFilterChanged(CategoryDto? value) => LoadBooksCommand.Execute(true);
        partial void OnSelectedAuthorFilterChanged(AuthorDto? value) => LoadBooksCommand.Execute(true);

        [RelayCommand]
        private async Task LoadFilterOptionsAsync()
        {
            await RunSafeAsync(async () =>
            {
                var catResponse = await _categoriesApi.GetCategories(null);
                if (catResponse.IsSuccessStatusCode && catResponse.Content != null)
                {
                    Categories.Clear();
                    Categories.Add(new CategoryDto { Id = Guid.Empty, Name = "All Categories" });
                    foreach (var cat in catResponse.Content.OrderBy(c => c.Name)) Categories.Add(cat);
                }
                var authResponse = await _authorApi.GetAuthors(null);
                if (authResponse.IsSuccessStatusCode && authResponse.Content != null)
                {
                    Authors.Clear();
                    Authors.Add(new AuthorDto { Id = Guid.Empty, Name = "All Authors" });
                    foreach (var auth in authResponse.Content.OrderBy(a => a.Name)) Authors.Add(auth);
                }
            }, nameof(ShowContent));
        }

        [RelayCommand]
        private async Task LoadBooksAsync()
        {
            try
            {
                IsBusy = true; // Ensure spinner starts
                _currentPage = 1;
                _canLoadMore = true;

                var response = await _booksApi.GetBooks(
                    SelectedCategoryFilter?.Id,
                    SelectedAuthorFilter?.Id,
                    SearchTerm,
                    _currentPage,
                    PageSize
                );

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Books.Clear();
                    foreach (var book in response.Content)
                        Books.Add(book);

                    _canLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load books.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load books");
                ErrorMessage = "Failed to load books. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadMoreBooksAsync()
        {
            if (_isLoadingMore || !_canLoadMore || IsBusy) return;
            _isLoadingMore = true;
            _currentPage++;
            _logger.LogInformation($"Loading page {_currentPage}");

            try
            {
                var response = await _booksApi.GetBooks(
                    SelectedCategoryFilter?.Id,
                    SelectedAuthorFilter?.Id,
                    SearchTerm,
                    _currentPage,
                    PageSize
                );

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var book in response.Content)
                        Books.Add(book);

                    _canLoadMore = response.Content.Count() == PageSize;
                }
            }
            finally
            {
                _isLoadingMore = false;
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            if (IsBusy) return;
            SelectedCategoryFilter = Categories.FirstOrDefault(c => c.Id == Guid.Empty);
            SelectedAuthorFilter = Authors.FirstOrDefault(a => a.Id == Guid.Empty);
            SearchTerm = string.Empty;
            OnPropertyChanged(nameof(SearchTerm));
        }


        [RelayCommand] private async Task SearchAsync() => await LoadBooksAsync();
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
                    await LoadBooksAsync(); // Reload
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

        public void OnAppearing()
        {
            if (!_hasLoaded)
            {
                _hasLoaded = true;
                LoadBooksCommand.Execute(false);
            }
        }

        public void OnDisappearing()
        {
            _hasLoaded = false;
        }
    }
}