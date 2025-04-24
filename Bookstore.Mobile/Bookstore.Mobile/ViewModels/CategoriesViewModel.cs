using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {
        private readonly ICategoriesApi _categoriesApi;
        private readonly ILogger<CategoriesViewModel> _logger;
        // private readonly INavigationService _navigationService;

        public CategoriesViewModel(ICategoriesApi categoriesApi, ILogger<CategoriesViewModel> logger /*, INavigationService navigationService*/)
        {
            _categoriesApi = categoriesApi;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "Categories";
            Categories = new ObservableCollection<CategoryDto>();
        }

        [ObservableProperty]
        private ObservableCollection<CategoryDto> _categories;

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Loading categories...");
                var response = await _categoriesApi.GetCategories();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Categories.Clear();
                    foreach (var category in response.Content)
                    {
                        Categories.Add(category);
                    }
                    _logger.LogInformation("Loaded {Count} categories.", Categories.Count);
                }
                else
                {
                    string errorContent = response.Error?.Content
                        ?? response.ReasonPhrase
                        ?? "Failed to load category.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load categories. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading categories.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToBooksForCategoryAsync(CategoryDto? selectedCategory)
        {
            if (IsBusy || selectedCategory == null) return;

            _logger.LogInformation("Navigating to Books Page for Category Id: {CategoryId}", selectedCategory.Id);
            // Điều hướng đến BooksPage và truyền CategoryId
            await Shell.Current.GoToAsync($"{nameof(BooksPage)}?CategoryId={selectedCategory.Id}");
            // await _navigationService.NavigateToAsync(nameof(BooksPage), new Dictionary<string, object> { { "CategoryId", selectedCategory.Id } });
        }

        public void OnAppearing()
        {
            // Load dữ liệu khi trang xuất hiện (nếu danh sách rỗng)
            if (Categories.Count == 0)
            {
                LoadCategoriesCommand.Execute(null);
            }
        }
    }
}