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
        private List<CategoryDto> _allCategories = new();

        public AdminCategoryListViewModel(ICategoriesApi categoriesApi, ILogger<AdminCategoryListViewModel> logger)
        {
            _categoriesApi = categoriesApi;
            _logger = logger;
            Title = "Manage Categories";
            Categories = new ObservableCollection<CategoryDto>();
        }

        [ObservableProperty] private ObservableCollection<CategoryDto> _categories;
        [ObservableProperty] private string? _searchTerm;
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && !HasError;

        partial void OnSearchTermChanged(string? value) => FilterCategories(value);

        [RelayCommand]
        private async Task LoadCategoriesAsync(object? parameter)
        {
            bool force = parameter is bool b && b;
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            try
            {
                if (force || !_allCategories.Any())
                {
                    _logger.LogInformation("Loading all categories for admin.");
                    var response = await _categoriesApi.GetCategories();
                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                        _allCategories = response.Content.OrderBy(c => c.Name).ToList();
                        _logger.LogInformation("Loaded {Count} total categories.", _allCategories.Count);
                    }
                    else
                    {
                        string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to load categories.";
                        ErrorMessage = $"Error: {errorContent}";
                        _logger.LogWarning("Failed to load categories. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                        _allCategories.Clear();
                    }
                }
                FilterCategories(SearchTerm); // Apply filter after loading/reloading
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading categories.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                _allCategories.Clear();
                Categories.Clear();
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        private void FilterCategories(string? searchTerm)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Categories.Clear();
                IEnumerable<CategoryDto> filtered = _allCategories;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filtered = _allCategories.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                     (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                }
                foreach (var cat in filtered) { Categories.Add(cat); }
                _logger.LogDebug("Filtered categories displayed: {Count}", Categories.Count);
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadCategoriesAsync(true);
        [RelayCommand] private async Task GoToAddCategoryAsync() => await Shell.Current.GoToAsync($"{nameof(AddEditCategoryPage)}?CategoryId={Guid.Empty}");
        [RelayCommand] private async Task GoToEditCategoryAsync(Guid? categoryId) { if (categoryId.HasValue) await Shell.Current.GoToAsync($"{nameof(AddEditCategoryPage)}?CategoryId={categoryId.Value}"); }

        [RelayCommand]
        private async Task DeleteCategoryAsync(Guid? categoryId)
        {
            if (!categoryId.HasValue || IsBusy) return;
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Delete this category?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                var response = await _categoriesApi.DeleteCategory(categoryId.Value);
                if (response.IsSuccessStatusCode)
                {
                    await LoadCategoriesAsync(true); // Reload list
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

        public void OnAppearing() { if (Categories.Count == 0) LoadCategoriesCommand.Execute(false); }
    }
}