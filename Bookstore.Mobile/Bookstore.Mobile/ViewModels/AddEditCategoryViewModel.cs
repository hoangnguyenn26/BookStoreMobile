using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(CategoryIdString), "CategoryId")]
    public partial class AddEditCategoryViewModel : BaseViewModel
    {
        private readonly ICategoriesApi _categoriesApi;
        private readonly ILogger<AddEditCategoryViewModel> _logger;

        private Guid _actualCategoryId = Guid.Empty;
        private string? _categoryIdString;

        public AddEditCategoryViewModel(ICategoriesApi categoriesApi, ILogger<AddEditCategoryViewModel> logger)
        {
            _categoriesApi = categoriesApi;
            _logger = logger;
            ParentCategories = new ObservableCollection<CategoryDto>();
            LoadParentCategoryOptionsCommand.Execute(null); // Load options khi khởi tạo
        }

        // Properties cho Binding
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCategoryCommand))] private string? _name;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private CategoryDto? _selectedParentCategory; // Lưu trữ CategoryDto được chọn
        [ObservableProperty] private ObservableCollection<CategoryDto> _parentCategories; // Danh sách cho Picker

        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowFormContent => !IsBusy && !HasError;

        public bool CanSaveCategoryPublic => CanSaveCategory();
        public string? CategoryIdString
        {
            get => _categoryIdString;
            set { if (_categoryIdString != value) { _categoryIdString = value; ProcessCategoryId(value); } }
        }

        private async void ProcessCategoryId(string? idString)
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                if (Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
                {
                    _actualCategoryId = parsedId;
                    Title = "Edit Category";

                    // Load parent categories first
                    await LoadParentCategoryOptionsAsync();

                    // Then load the category details
                    await LoadCategoryDetailsAsync(parsedId);
                }
                else
                {
                    _actualCategoryId = Guid.Empty;
                    Title = "Add New Category";

                    // Load parent categories for add mode too
                    await LoadParentCategoryOptionsAsync();
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to initialize form";
                _logger.LogError(ex, "Error processing category ID");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowFormContent));
                OnPropertyChanged(nameof(HasError));
            }
        }

        private void ResetForm() { Name = Description = null; SelectedParentCategory = ParentCategories.FirstOrDefault(c => c.Id == Guid.Empty); /* Chọn mục "None" */ }

        private async Task LoadCategoryDetailsAsync(Guid categoryIdToLoad)
        {
            try
            {
                var response = await _categoriesApi.GetCategoryById(categoryIdToLoad);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var category = response.Content;

                    Name = category.Name;
                    Description = category.Description;

                    if (ParentCategories.Any())
                    {
                        SelectedParentCategory = ParentCategories.FirstOrDefault(c => c.Id == category.ParentCategoryId)
                                               ?? ParentCategories.First();
                    }

                    ErrorMessage = null;
                    _logger.LogInformation("Loaded category details for {CategoryId}", categoryIdToLoad);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Category not found.";
                    _logger.LogWarning("Failed to load category {CategoryId}. Status: {StatusCode}",
                        categoryIdToLoad, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load category details.";
                _logger.LogError(ex, "Error loading category details for {CategoryId}", categoryIdToLoad);
            }
            finally
            {
                OnPropertyChanged(nameof(ShowFormContent));
            }
        }

        [RelayCommand]
        private async Task LoadParentCategoryOptionsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var response = await _categoriesApi.GetCategories();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    ParentCategories.Clear();
                    ParentCategories.Add(new CategoryDto { Id = Guid.Empty, Name = "- None -" });

                    foreach (var cat in response.Content.OrderBy(c => c.Name))
                    {
                        if (_actualCategoryId == Guid.Empty || cat.Id != _actualCategoryId)
                        {
                            ParentCategories.Add(cat);
                        }
                    }

                    if (_actualCategoryId != Guid.Empty && _selectedParentCategory != null)
                    {
                        SelectedParentCategory = ParentCategories.FirstOrDefault(c => c.Id == _selectedParentCategory.Id);
                    }
                }
                else
                {
                    ErrorMessage = "Failed to load parent categories.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading parent categories";
                _logger.LogError(ex, "Error loading parent categories");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowFormContent));
                OnPropertyChanged(nameof(HasError));
            }
        }


        private bool CanSaveCategory() => !string.IsNullOrWhiteSpace(Name) && IsNotBusy;

        [RelayCommand(CanExecute = nameof(CanSaveCategory))]
        private async Task SaveCategoryAsync()
        {
            IsBusy = true; ErrorMessage = null;
            try
            {
                bool success = false;
                ApiResponse<object>? response = null;
                ApiResponse<CategoryDto>? createResponse = null;
                Guid? parentId = (SelectedParentCategory?.Id == Guid.Empty) ? null : SelectedParentCategory?.Id;

                if (_actualCategoryId == Guid.Empty) // Add
                {
                    var createDto = new CreateCategoryDto { Name = Name!, Description = Description, ParentCategoryId = parentId };
                    createResponse = await _categoriesApi.CreateCategory(createDto);
                    success = createResponse.IsSuccessStatusCode;
                    if (!success) ErrorMessage = createResponse.Error?.Content ?? "Failed";
                }
                else // Update
                {
                    var updateDto = new UpdateCategoryDto { Name = Name!, Description = Description, ParentCategoryId = parentId };
                    response = await _categoriesApi.UpdateCategory(_actualCategoryId, updateDto);
                    success = response.IsSuccessStatusCode;
                    if (!success) ErrorMessage = response.Error?.Content ?? "Failed";
                }

                if (success)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else { await DisplayAlertAsync("Save Failed", ErrorMessage ?? "Could not save."); }
            }
            catch (Exception ex) { /* Log, DisplayAlert */ }
            finally { IsBusy = false; }
        }
    }
}