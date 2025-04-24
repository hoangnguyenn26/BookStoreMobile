// Bookstore.Mobile/ViewModels/HomeViewModel.cs
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services; // AuthService
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Bookstore.Mobile.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        // Inject Services
        private readonly IDashboardApi _dashboardApi;
        private readonly IAuthService _authService;
        private readonly ILogger<HomeViewModel> _logger;
        // private readonly INavigationService _navigationService; // Sẽ dùng sau

        public HomeViewModel(IDashboardApi dashboardApi, IAuthService authService, /*INavigationService navigationService,*/ ILogger<HomeViewModel> logger)
        {
            _dashboardApi = dashboardApi;
            _authService = authService;
            _logger = logger;
            // _navigationService = navigationService;
            Title = "Home";
            DashboardData = new HomeDashboardDto();
            UpdateWelcomeMessage();
        }

        [ObservableProperty]
        private HomeDashboardDto _dashboardData;

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        [ObservableProperty]
        private string _welcomeMessage = "Welcome!";

        [ObservableProperty]
        private bool _isLoggedIn;

        private void UpdateWelcomeMessage()
        {
            IsLoggedIn = _authService.IsLoggedIn; // Cập nhật trạng thái đăng nhập
            if (IsLoggedIn && _authService.CurrentUser != null)
            {
                WelcomeMessage = $"Hi, {_authService.CurrentUser.FirstName ?? _authService.CurrentUser.UserName}!";
            }
            else
            {
                WelcomeMessage = "Welcome to the Bookstore!";
            }
        }


        // Command để tải dữ liệu Dashboard
        [RelayCommand]
        private async Task LoadDashboardDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                _logger.LogInformation("Loading dashboard data...");
                var response = await _dashboardApi.GetHomeDashboard(); // Gọi API qua Refit

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    DashboardData = response.Content; // Gán dữ liệu vào property
                    _logger.LogInformation("Dashboard data loaded successfully.");
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to load dashboard data.";
                    ErrorMessage = $"Error: {errorContent}";
                    _logger.LogWarning("Failed to load dashboard data. Status: {StatusCode}, Reason: {Reason}", response.StatusCode, ErrorMessage);
                    // await DisplayAlertAsync("Error", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading dashboard data.");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                // await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Command để điều hướng đến chi tiết sách
        [RelayCommand]
        private async Task GoToBookDetailsAsync(Guid? bookId)
        {
            if (!bookId.HasValue || bookId.Value == Guid.Empty) return;
            _logger.LogInformation("Navigating to Book Details for Id: {BookId}", bookId.Value);
            // Dùng Shell Navigation để truyền tham số
            //await Shell.Current.GoToAsync($"{nameof(BookDetailsPage)}?BookId={bookId.Value}");
            // await _navigationService.NavigateToAsync(nameof(BookDetailsPage), new Dictionary<string, object> { { "BookId", bookId.Value } });
        }

        // Command để điều hướng đến danh sách sách của danh mục
        [RelayCommand]
        private async Task GoToCategoryAsync(Guid? categoryId)
        {
            if (!categoryId.HasValue || categoryId.Value == Guid.Empty) return;
            _logger.LogInformation("Navigating to Books Page for Category Id: {CategoryId}", categoryId.Value);
            //await Shell.Current.GoToAsync($"{nameof(BooksPage)}?CategoryId={categoryId.Value}");
            // await _navigationService.NavigateToAsync(nameof(BooksPage), new Dictionary<string, object> { { "CategoryId", categoryId.Value } });
        }

        // Gọi LoadDashboardDataAsync khi ViewModel được tạo hoặc trang xuất hiện
        // Có thể gọi trong constructor hoặc trong OnAppearing của Page
        public void OnAppearing() // Tạo phương thức này để Page gọi
        {
            // Cập nhật thông điệp chào mừng khi trang xuất hiện lại
            UpdateWelcomeMessage();
            // Chỉ tải dữ liệu nếu chưa có hoặc cần làm mới
            if (DashboardData.NewestBooks.Count == 0) // Ví dụ kiểm tra đơn giản
            {
                // Không await trực tiếp trong OnAppearing của ViewModel
                // Task.Run(async () => await LoadDashboardDataAsync()); // Chạy bất đồng bộ
                // Hoặc dùng Command để gọi từ OnAppearing của Page
                LoadDashboardDataCommand.Execute(null);
            }
        }
    }
}