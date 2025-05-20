using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using System.Collections.ObjectModel;
using Bookstore.Mobile.Helpers;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(BookIdString), "BookId")]
    public partial class AddEditBookViewModel : BaseViewModel
    {
        private readonly IBooksApi _booksApi;
        private readonly ICategoriesApi _categoriesApi;
        private readonly IAuthorApi _authorApi;
        private readonly ILogger<AddEditBookViewModel> _logger;
        // private readonly INavigationService _navigationService;

        private Guid _actualBookId = Guid.Empty;
        private string? _bookIdString;
        private bool _isDataLoaded = false;

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
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private string? _publicationYear;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private string? _price;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private string? _stockQuantity;
        [ObservableProperty] private string? _coverImageUrl; // Để hiển thị ảnh hiện tại

        [ObservableProperty] private ObservableCollection<CategoryDto> _categories;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveBookCommand))] private CategoryDto? _selectedCategory;

        [ObservableProperty] private ObservableCollection<AuthorDto> _authors;
        [ObservableProperty] private AuthorDto? _selectedAuthor;

        [ObservableProperty] private string? _errorMessage;
        public bool CanSaveBookPublic => CanSaveBook();

        [ObservableProperty]
        private bool _isUploadingImage;

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
            OnPropertyChanged(nameof(ShowContent));

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
            OnPropertyChanged(nameof(ShowContent));
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
                    Authors.Add(new AuthorDto { Id = Guid.Empty, Name = " - No Author - " });
                    foreach (var auth in authTask.Result.Content.OrderBy(a => a.Name)) Authors.Add(auth);
                    SelectedAuthor = Authors.FirstOrDefault();
                }
                else { _logger.LogWarning("Failed to load authors for picker."); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading picker options.");
                ErrorMessage = "Failed to load category/author options.";
                OnPropertyChanged(nameof(HasError));
            }
        }

        //Điều kiện để save
        private bool CanSaveBook() =>
                     !string.IsNullOrWhiteSpace(BookTitle) &&
                     SelectedCategory != null && SelectedCategory.Id != Guid.Empty &&
                     decimal.TryParse(Price, out _) && decimal.Parse(Price) >= 0 &&
                     int.TryParse(StockQuantity, out _) && int.Parse(StockQuantity) >= 0 &&
                     (!int.TryParse(PublicationYear, out int year) || year > 1000 && year <= DateTime.Now.Year + 1) &&
                     IsNotBusy && !IsUploadingImage; // <<-- Thêm điều kiện không đang upload ảnh

        [RelayCommand(CanExecute = nameof(CanSaveBook))]
        private async Task SaveBookAsync()
        {
            if (!CanSaveBook())
            {
                await DisplayAlertAsync("Validation Error", "Please fill in all required fields correctly.", "OK");
                return;
            }

            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Attempting to save book (Actual Id: {BookId})", _actualBookId);

            try
            {
                bool success = false;
                ApiResponse<object>? response = null; // Dùng chung cho PUT/DELETE
                ApiResponse<BookDto>? createResponse = null; // Riêng cho POST

                // --- Chuẩn bị DTO ---
                // Parse các giá trị số từ string
                if (!decimal.TryParse(Price, out decimal priceValue)) priceValue = 0;
                if (!int.TryParse(StockQuantity, out int stockValue)) stockValue = 0;
                int? publicationYearValue = int.TryParse(PublicationYear, out int year) ? year : null;

                // Lấy Id từ Picker đã chọn
                Guid categoryIdValue = SelectedCategory?.Id ?? Guid.Empty;
                Guid? authorIdValue = (SelectedAuthor?.Id == Guid.Empty) ? null : SelectedAuthor?.Id;

                if (_actualBookId == Guid.Empty)
                {
                    var createDto = new CreateBookDto
                    {
                        Title = BookTitle!,
                        Description = Description,
                        ISBN = ISBN,
                        Publisher = Publisher,
                        PublicationYear = publicationYearValue,
                        Price = priceValue,
                        StockQuantity = stockValue,
                        CategoryId = categoryIdValue,
                        AuthorId = authorIdValue
                        // CoverImageUrl sẽ được xử lý riêng qua upload
                    };
                    _logger.LogInformation("Calling CreateBook API...");
                    createResponse = await _booksApi.CreateBook(createDto);
                    success = createResponse.IsSuccessStatusCode;
                    if (!success)
                    {
                        string errorContent = "Unknown error";
                        if (createResponse?.Error?.Content != null)
                        {
                            // Thử đọc như chuỗi trước
                            if (createResponse.Error.Content is string strContent)
                            {
                                errorContent = ErrorMessageHelper.ToFriendlyErrorMessage(strContent);
                            }
                            else
                            {
                                errorContent = createResponse.ReasonPhrase ?? "Failed to create book";
                            }
                        }
                        else // Nếu không có Error hoặc Content, dùng ReasonPhrase
                        {
                            errorContent = createResponse?.ReasonPhrase ?? "Failed to create book";
                        }
                        ErrorMessage = errorContent;
                    }
                }
                else // CẬP NHẬT
                {
                    var updateDto = new UpdateBookDto
                    {
                        Title = BookTitle!,
                        Description = Description,
                        ISBN = ISBN,
                        Publisher = Publisher,
                        PublicationYear = publicationYearValue,
                        Price = priceValue,
                        StockQuantity = stockValue,
                        CategoryId = categoryIdValue,
                        AuthorId = authorIdValue
                    };
                    _logger.LogInformation("Calling UpdateBook API for BookId {BookId}...", _actualBookId);
                    response = await _booksApi.UpdateBook(_actualBookId, updateDto);
                    success = response.IsSuccessStatusCode;
                    if (!success)
                    {
                        string errorContent = "Unknown error";
                        if (response?.Error?.Content != null)
                        {
                            if (response.Error.Content is string strContent) { errorContent = strContent; }
                            else { errorContent = response.ReasonPhrase ?? "Failed to update book"; }
                        }
                        else { errorContent = response?.ReasonPhrase ?? "Failed to update book"; }
                        ErrorMessage = ErrorMessageHelper.ToFriendlyErrorMessage(errorContent);
                    }
                }

                // --- Xử lý kết quả ---
                if (success)
                {
                    _logger.LogInformation("Book saved successfully (Id: {BookId})", _actualBookId == Guid.Empty ? (createResponse?.Content?.Id.ToString() ?? "(New)") : _actualBookId.ToString());
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    _logger.LogWarning("Failed to save book {BookId}. Reason: {Reason}", _actualBookId, ErrorMessage);
                    await DisplayAlertAsync("Save Failed", ErrorMessage ?? "Could not save the book.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while saving book {BookId}", _actualBookId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanUploadImage))]
        private async Task UploadCoverImageAsync()
        {
            if (_actualBookId == Guid.Empty || IsBusy) return;

            IsUploadingImage = true;
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                _logger.LogInformation("Initiating cover image upload for Book {BookId}", _actualBookId);
                // Sử dụng MediaPicker để chọn ảnh
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult? photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                    {
                        Title = "Select Book Cover"
                    });

                    if (photo != null)
                    {
                        _logger.LogInformation("Photo picked: {FileName}", photo.FileName);
                        // Lấy Stream từ FileResult
                        using var sourceStream = await photo.OpenReadAsync();

                        var filePart = new StreamPart(sourceStream, photo.FileName, photo.ContentType);

                        _logger.LogInformation("Calling UploadBookCoverImage API for BookId {BookId}...", _actualBookId);
                        // Gọi API Upload ảnh
                        var response = await _booksApi.UploadBookCoverImage(_actualBookId, filePart);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Cover image uploaded successfully for Book {BookId}. Reloading details...", _actualBookId);
                            // Thành công! Cần load lại chi tiết sách để cập nhật CoverImageUrl mới
                            await LoadBookDetailsAsync(_actualBookId); // Load lại để thấy ảnh mới
                            await DisplayAlertAsync("Success", "Cover image uploaded successfully!", "OK");
                        }
                        else
                        {
                            string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to upload image.";
                            ErrorMessage = $"Upload Error: {errorContent}";
                            _logger.LogWarning("Failed to upload cover image for Book {BookId}. Status: {StatusCode}, Reason: {Reason}", _actualBookId, response.StatusCode, ErrorMessage);
                            await DisplayAlertAsync("Upload Failed", ErrorMessage);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No photo selected by user.");
                    }
                }
                else
                {
                    ErrorMessage = "Photo capture/picking is not supported on this device.";
                    _logger.LogWarning(ErrorMessage);
                    await DisplayAlertAsync("Not Supported", ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during cover image upload for Book {BookId}", _actualBookId);
                ErrorMessage = $"An unexpected error occurred during upload: {ex.Message}";
                await DisplayAlertAsync("Upload Error", ErrorMessage);
            }
            finally
            {
                IsUploadingImage = false;
                IsBusy = false;
            }
        }

        // Điều kiện để có thể Upload ảnh
        private bool CanUploadImage() => _actualBookId != Guid.Empty;
    }
}