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
        private bool _hasLoaded = false;

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

        partial void OnSearchTermChanged(string? value) => FilterAuthors(value);

        [RelayCommand]
        private async Task LoadAuthorsAsync(bool isRefresh = false)
        {
            await RunSafeAsync(async () =>
            {
                if (isRefresh)
                {
                    _currentPage = 1;
                    _canLoadMore = true;
                    Authors.Clear();
                }

                var response = await _authorApi.GetAuthors(_currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var author in response.Content.OrderBy(a => a.Name))
                        Authors.Add(author);
                    _canLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load authors.";
                }
            }, showBusy: true);
        }

        [RelayCommand]
        private async Task LoadMoreAuthorsAsync()
        {
            if (IsBusy || !_canLoadMore) return;

            await RunSafeAsync(async () =>
            {
                _currentPage++;
                var response = await _authorApi.GetAuthors(_currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var author in response.Content.OrderBy(a => a.Name))
                        Authors.Add(author);
                    _canLoadMore = response.Content.Count() == PageSize;
                }
            }, showBusy: true);
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadAuthorsAsync(true);

        private void FilterAuthors(string? searchTerm)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    LoadAuthorsCommand.Execute(false);
                    return;
                }

                var filtered = Authors
                    .Where(a => a.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Authors.Clear();
                foreach (var auth in filtered)
                {
                    Authors.Add(auth);
                }
            });
        }

        [RelayCommand]
        private async Task GoToAddAuthorAsync() =>
            await Shell.Current.GoToAsync($"{nameof(AddEditAuthorPage)}?AuthorId={Guid.Empty}");

        [RelayCommand]
        private async Task GoToEditAuthorAsync(Guid? authorId)
        {
            if (authorId.HasValue)
                await Shell.Current.GoToAsync($"{nameof(AddEditAuthorPage)}?AuthorId={authorId.Value}");
        }

        [RelayCommand]
        private async Task DeleteAuthorAsync(Guid? authorId)
        {
            if (!authorId.HasValue || IsBusy) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Delete this author?",
                "Yes",
                "No");
            if (!confirm) return;

            await RunSafeAsync(async () =>
            {
                var response = await _authorApi.DeleteAuthor(authorId.Value);
                if (response.IsSuccessStatusCode)
                {
                    var toRemove = Authors.FirstOrDefault(a => a.Id == authorId.Value);
                    if (toRemove != null)
                        Authors.Remove(toRemove);
                }
                else
                {
                    throw new Exception(response.Error?.Content ?? "Failed to delete author");
                }
            }, showBusy: true);
        }

        public void OnAppearing()
        {
            if (!_hasLoaded)
            {
                _hasLoaded = true;
                LoadAuthorsCommand.Execute(false);
            }
        }

        public void OnDisappearing() => _hasLoaded = false;
    }
}