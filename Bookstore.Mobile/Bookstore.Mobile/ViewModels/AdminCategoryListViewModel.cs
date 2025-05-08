using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminCategoryListViewModel : BaseViewModel
    {
        private readonly ICategoriesApi _categoriesApi;
        private readonly ILogger<AdminCategoryListViewModel> _logger;
        private int _currentPage = 1;
        private const int PageSize = 15;
        private bool _canLoadMore = true;
        private bool _hasLoaded = false;

        public AdminCategoryListViewModel(ICategoriesApi categoriesApi, ILogger<AdminCategoryListViewModel> logger)
        {
            _categoriesApi = categoriesApi;
            _logger = logger;
            Title = "Manage Categories";
            Categories = new ObservableCollection<CategoryDto>();
        }

        [ObservableProperty]
        private ObservableCollection<CategoryDto> _categories;

        [ObservableProperty]
        private string? _searchTerm;

        partial void OnSearchTermChanged(string? value) => FilterCategories(value);

        [RelayCommand]
        private async Task LoadCategoriesAsync(bool isRefresh = false)
        {
            await RunSafeAsync(async () =>
            {
                if (isRefresh)
                {
                    _currentPage = 1;
                    _canLoadMore = true;
                    Categories.Clear();
                }

                var response = await _categoriesApi.GetCategories(_currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var cat in response.Content.OrderBy(c => c.Name))
                        Categories.Add(cat);
                    _canLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load categories.";
                }
            }, showBusy: true);
        }

        [RelayCommand]
        private async Task LoadMoreCategoriesAsync()
        {
            if (IsBusy || !_canLoadMore) return;

            await RunSafeAsync(async () =>
            {
                _currentPage++;
                var response = await _categoriesApi.GetCategories(_currentPage, PageSize);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var cat in response.Content.OrderBy(c => c.Name))
                        Categories.Add(cat);
                    _canLoadMore = response.Content.Count() == PageSize;
                }
            }, showBusy: true);
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadCategoriesAsync(true);

        private void FilterCategories(string? searchTerm)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    LoadCategoriesCommand.Execute(false);
                    return;
                }

                var filtered = Categories
                    .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                               (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

                Categories.Clear();
                foreach (var cat in filtered)
                {
                    Categories.Add(cat);
                }
            });
        }

        [RelayCommand]
        private async Task GoToAddCategoryAsync() =>
            await Shell.Current.GoToAsync($"{nameof(AddEditCategoryPage)}?CategoryId={Guid.Empty}");

        [RelayCommand]
        private async Task GoToEditCategoryAsync(Guid? categoryId)
        {
            if (categoryId.HasValue)
                await Shell.Current.GoToAsync($"{nameof(AddEditCategoryPage)}?CategoryId={categoryId.Value}");
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(Guid? categoryId)
        {
            if (!categoryId.HasValue || IsBusy) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Delete this category?",
                "Yes",
                "No");
            if (!confirm) return;

            await RunSafeAsync(async () =>
            {
                var response = await _categoriesApi.DeleteCategory(categoryId.Value);
                if (response.IsSuccessStatusCode)
                {
                    var toRemove = Categories.FirstOrDefault(c => c.Id == categoryId.Value);
                    if (toRemove != null)
                        Categories.Remove(toRemove);
                }
                else
                {
                    throw new Exception(response.Error?.Content ?? "Failed to delete category");
                }
            }, showBusy: true);
        }

        public void OnAppearing()
        {
            if (!_hasLoaded)
            {
                _hasLoaded = true;
                LoadCategoriesCommand.Execute(false);
            }
        }

        public void OnDisappearing() => _hasLoaded = false;
    }
}