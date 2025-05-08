using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;

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

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            await RunSafeAsync(async () =>
            {
                var response = await _categoriesApi.GetCategories();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Categories.Clear();
                    foreach (var cat in response.Content.OrderBy(c => c.Name))
                        Categories.Add(cat);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load categories.";
                }
            }, nameof(ShowContent));
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