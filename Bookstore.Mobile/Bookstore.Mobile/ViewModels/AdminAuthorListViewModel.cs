// Bookstore.Mobile/ViewModels/AdminAuthorListViewModel.cs
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
        private List<AuthorDto> _allAuthors = new();

        public AdminAuthorListViewModel(IAuthorApi authorApi, ILogger<AdminAuthorListViewModel> logger)
        {
            _authorApi = authorApi;
            _logger = logger;
            Title = "Manage Authors";
            Authors = new ObservableCollection<AuthorDto>();
        }

        [ObservableProperty] private ObservableCollection<AuthorDto> _authors;
        [ObservableProperty] private string? _searchTerm;
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && !HasError;

        partial void OnSearchTermChanged(string? value) => FilterAuthors(value);

        [RelayCommand]
        private async Task LoadAuthorsAsync(object? parameter)
        {
            bool force = parameter is bool b && b;
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                if (force || !_allAuthors.Any())
                {
                    var response = await _authorApi.GetAuthors();
                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                        _allAuthors = response.Content.OrderBy(a => a.Name).ToList();
                    }
                    else
                    {
                        ErrorMessage = response.Error?.Content ?? "Failed to load";
                        _allAuthors.Clear();
                    }
                }
                FilterAuthors(SearchTerm);
            }
            catch (Exception ex) { ErrorMessage = ex.Message; _allAuthors.Clear(); Authors.Clear(); }
            finally { IsBusy = false; OnPropertyChanged(nameof(ShowContent)); }
        }

        private void FilterAuthors(string? searchTerm)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Authors.Clear();
                IEnumerable<AuthorDto> filtered = _allAuthors;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filtered = _allAuthors.Where(a => a.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }
                foreach (var auth in filtered) { Authors.Add(auth); }
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAuthorsAsync(true);
        [RelayCommand] private async Task GoToAddAuthorAsync() => await Shell.Current.GoToAsync($"{nameof(AddEditAuthorPage)}?AuthorId={Guid.Empty}");
        [RelayCommand] private async Task GoToEditAuthorAsync(Guid? authorId) { if (authorId.HasValue) await Shell.Current.GoToAsync($"{nameof(AddEditAuthorPage)}?AuthorId={authorId.Value}"); }

        [RelayCommand]
        private async Task DeleteAuthorAsync(Guid? authorId)
        {
            if (!authorId.HasValue || IsBusy) return;
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Delete this author?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                var response = await _authorApi.DeleteAuthor(authorId.Value);
                if (response.IsSuccessStatusCode)
                {
                    await LoadAuthorsAsync(true); // Reload
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

        public void OnAppearing() { if (Authors.Count == 0) LoadAuthorsCommand.Execute(false); }
    }
}