using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(BookIdString), "BookId")] // Nhận string
    public partial class AddEditBookViewModel : BaseViewModel
    {
        private readonly IBooksApi _booksApi;
        private readonly ICategoriesApi _categoriesApi;
        private readonly IAuthorApi _authorApi;
        private readonly ILogger<AddEditBookViewModel> _logger;
        // private readonly INavigationService _navigationService;

        private Guid _actualBookId = Guid.Empty;
        private string? _bookIdString;
        private bool _isDataLoaded = false; // Cờ tránh load lại khi không cần

        public AddEditBookViewModel(IBooksApi booksApi, ICategoriesApi categoriesApi, IAuthorApi authorApi, ILogger<AddEditBookViewModel> logger)
        {
            _booksApi = booksApi;
            _categoriesApi = categoriesApi;
            _authorApi = authorApi;
            _logger = logger;
            Categories = new ObservableCollection<CategoryDto>();
            Authors = new ObservableCollection<AuthorDto>();
            // Load options cho Picker khi ViewModel được tạo
            LoadPickerOptionsCommand.Execute(null);
        }

        // ---- Properties cho Binding ----
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private string? _bookTitle;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private string? _iSBN;
        [ObservableProperty] private string? _publisher;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private string? _publicationYear; // Dùng string để binding Entry dễ hơn
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private string? _price; // Dùng string
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private string? _stockQuantity; // Dùng string
        [ObservableProperty] private string? _coverImageUrl; // Để hiển thị ảnh hiện tại

        [ObservableProperty] private ObservableCollection<CategoryDto> _categories;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private CategoryDto? _selectedCategory;

        [ObservableProperty] private ObservableCollection<AuthorDto> _authors;
        [ObservableProperty] private AuthorDto? _selectedAuthor;

        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        // Chỉ hiện form khi không bận và không có lỗi load ban đầu
        public bool ShowFormContent => !IsBusy && !HasError && _isDataLoaded;

        // Property nhận giá trị string từ QueryProperty
        public string? BookIdString
        {
            get => _bookIdString;
            set
            {
                if (_bookIdString != value)
                {
                    _bookIdString = value;
                    ProcessBookId(value);
                }
            }
        }

        // --- Logic xử lý Id và Load Data ---
        private async void ProcessBookId(string? idString)
        {
            IsBusy = true; // Báo hiệu đang xử lý Id và load data
            ErrorMessage = null;
            _isDataLoaded = false;
            OnPropertyChanged(nameof(ShowFormContent));

            if (Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
            {
                _actualBookId = parsedId;
                Title = "Edit Book";
                _logger.LogInformation("Processing Edit mode for BookId: {BookId}", _actualBookId);
                await LoadBookDetailsAsync(_actualBookId);
            }
            else
            {
                _actualBookId = Guid.Empty;
                Title = "Add New Book";
                ResetForm();
                _logger.LogInformation("Processing Add New Book mode.");
                _isDataLoaded = true; // Đánh dấu đã sẵn sàng (vì không cần load sách cũ)
            }
            IsBusy = false;
            OnPropertyChanged(nameof(ShowFormContent));
        }

        private void ResetForm()
        {
            BookTitle = Description = ISBN = Publisher = PublicationYear = Price = StockQuantity = CoverImageUrl = null;
            SelectedCategory = Categories.FirstOrDefault();
            SelectedAuthor = Authors.FirstOrDefault();
        }

        private async Task LoadBookDetailsAsync(Guid bookIdToLoad)
        {
            try
            {
                var response = await _booksApi.GetBookById(bookIdToLoad);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var book = response.Content;
                    // Điền dữ liệu vào các property
                    BookTitle = book.Title;
                    Description = book.Description;
                    ISBN = book.ISBN;
                    Publisher = book.Publisher;
                    PublicationYear = book.PublicationYear?.ToString();
                    Price = book.Price.ToString("F2"); // Format số thập phân
                    StockQuantity = book.StockQuantity.ToString();
                    CoverImageUrl = book.CoverImageUrl;

                    // Chọn Category và Author tương ứng trong Picker
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == book.CategoryId);
                    SelectedAuthor = Authors.FirstOrDefault(a => a.Id == book.Author.Id);

                    _isDataLoaded = true;
                    _logger.LogInformation("Book details loaded for editing.");
                }
                else
                {
                    ErrorMessage = "Failed to load book details for editing.";
                    _logger.LogWarning("Failed to load book {BookId} for editing. Status: {StatusCode}", bookIdToLoad, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book details for {BookId}", bookIdToLoad);
                ErrorMessage = "An error occurred while loading book details.";
            }
        }

        [RelayCommand]
        private async Task LoadPickerOptionsAsync()
        {
            if (IsBusy) return;
            try
            {
                _logger.LogInformation("Loading category and author options for pickers.");
                var catTask = _categoriesApi.GetCategories();
                var authTask = _authorApi.GetAuthors();

                await Task.WhenAll(catTask, authTask);

                // Xử lý Categories
                if (catTask.Result.IsSuccessStatusCode && catTask.Result.Content != null)
                {
                    Categories.Clear();
                    // Không thêm "All" cho Picker này
                    foreach (var cat in catTask.Result.Content.OrderBy(c => c.Name)) Categories.Add(cat);
                    // Chọn mặc định nếu có thể (hoặc không chọn gì)
                    SelectedCategory = Categories.FirstOrDefault();
                }
                else { _logger.LogWarning("Failed to load categories for picker."); }

                // Xử lý Authors
                if (authTask.Result.IsSuccessStatusCode && authTask.Result.Content != null)
                {
                    Authors.Clear();
                    Authors.Add(new AuthorDto { Id = Guid.Empty, Name = " - No Author - " }); // Option không chọn Author
                    foreach (var auth in authTask.Result.Content.OrderBy(a => a.Name)) Authors.Add(auth);
                    // Chọn mặc định nếu có thể
                    SelectedAuthor = Authors.FirstOrDefault();
                }
                else { _logger.LogWarning("Failed to load authors for picker."); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading picker options.");
                ErrorMessage = "Failed to load category/author options.";
                OnPropertyChanged(nameof(HasError)); // Cập nhật HasError
            }
        }

        // Điều kiện để Save
        private bool CanSaveBook() =>
            !string.IsNullOrWhiteSpace(BookTitle) &&
            SelectedCategory != null && SelectedCategory.Id != Guid.Empty && // Phải chọn Category hợp lệ
            decimal.TryParse(Price, out _) && decimal.Parse(Price) >= 0 &&     // Giá hợp lệ
            int.TryParse(StockQuantity, out _) && int.Parse(StockQuantity) >= 0 && // Số lượng hợp lệ
            (!int.TryParse(PublicationYear, out int year) || year > 1000 && year <= DateTime.Now.Year + 1) && // Năm XB hợp lệ
            IsNotBusy;

        [RelayCommand(CanExecute = nameof(CanSaveBook))]
        private async Task SaveBookAsync() { await Task.CompletedTask; }

        [RelayCommand]
        private async Task UploadCoverImageAsync() { await Task.CompletedTask; }
    }
}