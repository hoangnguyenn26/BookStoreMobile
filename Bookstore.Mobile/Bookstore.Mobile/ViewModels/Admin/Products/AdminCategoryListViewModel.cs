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
        private string? _lastSearchTerm;

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

        [ObservableProperty]
        private bool _isRefreshing; // New property for RefreshView state

        [RelayCommand]
        private async Task LoadCategories(bool isRefreshing = false)
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
                    Categories.Clear();
                    _canLoadMore = true;
                }

                var response = await _categoriesApi.GetCategories(SearchTerm, _currentPage, PageSize);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var cat in response.Content.OrderBy(c => c.Name))
                    {
                        if (!Categories.Any(c => c.Id == cat.Id))
                            Categories.Add(cat);
                    }

                    _currentPage++;
                    _canLoadMore = response.Content.Count() == PageSize;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load categories.";
                }

                _lastSearchTerm = SearchTerm;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
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
            await LoadCategories();
        }

        [RelayCommand]
        private async Task Refresh() => await LoadCategories(true);

        partial void OnSearchTermChanged(string? value)
        {
            LoadCategoriesCommand.Execute(false);
        }

        [RelayCommand]
        private async Task GoToAddCategory() =>
            await Shell.Current.GoToAsync($"{nameof(AddEditCategoryPage)}?CategoryId={Guid.Empty}");

        [RelayCommand]
        private async Task GoToEditCategory(Guid? categoryId)
        {
            if (categoryId.HasValue)
                await Shell.Current.GoToAsync($"{nameof(AddEditCategoryPage)}?CategoryId={categoryId.Value}");
        }

        [RelayCommand]
        private async Task DeleteCategory(Guid? categoryId)
        {
            if (!categoryId.HasValue || IsBusy) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Delete this category?",
                "Yes",
                "No");
            if (!confirm) return;

            try
            {
                IsBusy = true;
                var response = await _categoriesApi.DeleteCategory(categoryId.Value);
                if (response.IsSuccessStatusCode)
                {
                    var toRemove = Categories.FirstOrDefault(c => c.Id == categoryId.Value);
                    if (toRemove != null)
                        Categories.Remove(toRemove);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to delete category";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing() => LoadCategoriesCommand.Execute(false);
    }
}