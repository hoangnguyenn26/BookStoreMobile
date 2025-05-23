using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminAuthorListViewModel : BaseViewModel
    {
        private readonly IAuthorApi _authorApi;
        private readonly ILogger<AdminAuthorListViewModel> _logger;
        private int _currentPage = 1;
        private const int PageSize = 15;
        private bool _canLoadMore = true;
        private string? _lastSearchTerm;

        public AdminAuthorListViewModel(IAuthorApi authorApi, ILogger<AdminAuthorListViewModel> logger)
        {
            _authorApi = authorApi;
            _logger = logger;
            Title = "Manage Authors";
            Authors = new ObservableCollection<AuthorDto>();
        }

        [ObservableProperty]
        private ObservableCollection<AuthorDto> _authors;

        [ObservableProperty]
        private string? _searchTerm;

        [ObservableProperty]
        private bool _isRefreshing; // New property for RefreshView state

        [RelayCommand]
        private async Task LoadAuthors(bool isRefreshing = false)
        {
            if (IsBusy && !isRefreshing)
                return;

            try
            {
                IsBusy = true;
                if (isRefreshing)
                    IsRefreshing = true; // Set only for refresh

                bool isSearchChanged = _lastSearchTerm != SearchTerm;
                if (isRefreshing || isSearchChanged)
                {
                    _currentPage = 1;
                    Authors.Clear();
                    _canLoadMore = true;
                }

                var response = await _authorApi.GetAuthors(SearchTerm, _currentPage, PageSize);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var author in response.Content.OrderBy(a => a.Name))
                        Authors.Add(author);

                    _currentPage++;
                    _canLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load authors.";
                }

                _lastSearchTerm = SearchTerm;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading authors");
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                if (isRefreshing)
                    IsRefreshing = false; // Reset only for refresh
            }
        }

        [RelayCommand]
        private async Task LoadMore()
        {
            if (!_canLoadMore || IsBusy) return;
            await LoadAuthors();
        }

        [RelayCommand]
        private async Task Refresh() => await LoadAuthors(true);

        partial void OnSearchTermChanged(string? value)
        {
            LoadAuthorsCommand.Execute(false);
        }

        [RelayCommand]
        private async Task GoToAddAuthor() =>
            await Shell.Current.GoToAsync($"{nameof(AddEditAuthorPage)}?AuthorId={Guid.Empty}");

        [RelayCommand]
        private async Task GoToEditAuthor(Guid? authorId)
        {
            if (authorId.HasValue)
                await Shell.Current.GoToAsync($"{nameof(AddEditAuthorPage)}?AuthorId={authorId.Value}");
        }

        [RelayCommand]
        private async Task DeleteAuthor(Guid? authorId)
        {
            if (!authorId.HasValue || IsBusy) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Delete this author?",
                "Yes",
                "No");
            if (!confirm) return;

            try
            {
                IsBusy = true;
                var response = await _authorApi.DeleteAuthor(authorId.Value);
                if (response.IsSuccessStatusCode)
                {
                    var toRemove = Authors.FirstOrDefault(a => a.Id == authorId.Value);
                    if (toRemove != null)
                        Authors.Remove(toRemove);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to delete author";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author");
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing() => LoadAuthorsCommand.Execute(false);
    }
}