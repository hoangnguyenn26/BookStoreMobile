using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminUserListViewModel : BaseViewModel
    {
        private readonly IAdminUserApi _userApi;
        private readonly ILogger<AdminUserListViewModel> _logger;
        private CancellationTokenSource _filterDebounceCts = new();

        // Paging & Loading State
        private int _currentPage = 1;
        private const int PageSize = 15;
        private bool _isLoadingMore = false;
        private bool _canLoadMore = true;

        // Filter State
        public ObservableCollection<string> AvailableRoles { get; }
        public ObservableCollection<string> AvailableStatuses { get; }

        [ObservableProperty]
        private string _selectedRoleFilter = "All";

        [ObservableProperty]
        private string _selectedStatusFilter = "All";

        // Data & UI State
        [ObservableProperty]
        private ObservableCollection<UserDto> _users;

        [ObservableProperty]
        private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowContent => !IsBusy && !HasError && !_initialLoadPending;
        public bool HasFiltersApplied => SelectedRoleFilter != "All" || SelectedStatusFilter != "All";
        private bool _initialLoadPending = true;

        public AdminUserListViewModel(IAdminUserApi userApi, ILogger<AdminUserListViewModel> logger)
        {
            _userApi = userApi ?? throw new ArgumentNullException(nameof(userApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Title = "Manage Users";
            Users = new ObservableCollection<UserDto>();
            AvailableRoles = new ObservableCollection<string> { "All", "Admin", "Staff", "User" };
            AvailableStatuses = new ObservableCollection<string> { "All", "Active", "Inactive" };
        }

        // Xử lý khi Filter thay đổi -> Tải lại trang đầu sau một khoảng trễ
        async partial void OnSelectedRoleFilterChanged(string value) => await DebouncedFilterChange();
        async partial void OnSelectedStatusFilterChanged(string value) => await DebouncedFilterChange();

        private async Task DebouncedFilterChange()
        {
            try
            {
                _filterDebounceCts.Cancel();
                _filterDebounceCts.Dispose();
                _filterDebounceCts = new CancellationTokenSource();

                await Task.Delay(500, _filterDebounceCts.Token);

                // Chỉ gọi Load nếu không bị cancel
                _logger.LogDebug("Debounce timer elapsed, triggering user load.");
                await LoadUsersAsync(true);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Filter change debounced/cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debounced filter change.");
            }
        }

        [RelayCommand]
        private async Task ClearFilters()
        {
            if (HasFiltersApplied)
            {
                _logger.LogInformation("Clearing filters.");
                SelectedRoleFilter = "All";
                SelectedStatusFilter = "All";
            }
        }

        // Modified to handle both parameter types
        [RelayCommand]
        private async Task LoadUsersAsync(object isRefreshingParam)
        {
            // Convert parameter to boolean, handling both string and boolean types
            bool isRefreshing = false;
            if (isRefreshingParam is bool boolValue)
            {
                isRefreshing = boolValue;
            }
            else if (isRefreshingParam is string stringValue)
            {
                bool.TryParse(stringValue, out isRefreshing);
            }

            try
            {
                // Kiểm tra các cờ trạng thái
                if (_isLoadingMore || (!isRefreshing && !_canLoadMore))
                {
                    _logger.LogDebug("LoadUsersAsync skipped: Already loading more or no more items.");
                    return;
                }
                if (!isRefreshing && IsBusy)
                {
                    _logger.LogDebug("LoadUsersAsync skipped: IsBusy is true and not refreshing.");
                    return;
                }

                IsBusy = true;
                if (isRefreshing)
                {
                    _logger.LogInformation("Refreshing user list. Filters - Role: {Role}, Status: {Status}", SelectedRoleFilter, SelectedStatusFilter);
                    _currentPage = 1;
                    Users.Clear();
                    _canLoadMore = true;
                }
                ErrorMessage = null;

                // Thực hiện gọi API
                await ExecuteLoadUsers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoadUsersAsync");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                Users.Clear(); // Xóa dữ liệu khi có lỗi nghiêm trọng
            }
            finally
            {
                IsBusy = false;
                _isLoadingMore = false;
                _initialLoadPending = false;
                OnPropertyChanged(nameof(ShowContent));
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(HasFiltersApplied));
            }
        }

        private async Task ExecuteLoadUsers()
        {
            string? roleFilter = SelectedRoleFilter == "All" ? null : SelectedRoleFilter;
            bool? statusFilter = SelectedStatusFilter == "All" ? null : SelectedStatusFilter == "Active";

            _logger.LogDebug("Executing API call to load users. Page: {Page}, Role: {Role}, Status: {Status}",
                _currentPage, roleFilter ?? "All", statusFilter?.ToString() ?? "All");

            try
            {
                // Gọi API với các tham số đã chuẩn bị
                var response = await _userApi.GetUsers(_currentPage, PageSize, roleFilter, statusFilter);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var newUsers = response.Content.ToList();
                    // Sử dụng Dispatcher để đảm bảo cập nhật UI trên đúng luồng
                    if (Application.Current?.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Dispatch(() =>
                        {
                            foreach (var user in newUsers)
                            {
                                Users.Add(user);
                            }
                        });
                    }
                    else
                    {
                        foreach (var user in newUsers) Users.Add(user);
                    }


                    _currentPage++;
                    _canLoadMore = newUsers.Count == PageSize;

                    _logger.LogInformation("Loaded {Count} users. Total: {Total}, Can load more: {CanLoadMore}",
                        newUsers.Count, Users.Count, _canLoadMore);
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown API error";
                    ErrorMessage = $"Failed to load users: {errorContent}";
                    _logger.LogWarning("API error loading users. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    Users.Clear(); // Xóa dữ liệu hiện tại nếu API lỗi
                }
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx, "API Exception loading users. Status: {StatusCode}, Content: {Content}", apiEx.StatusCode, apiEx.Content);
                ErrorMessage = $"API Error ({apiEx.StatusCode}): {apiEx.ReasonPhrase} {apiEx.Content}";
                Users.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during ExecuteLoadUsers");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                Users.Clear();
            }
            finally
            {
                OnPropertyChanged(nameof(HasError));
            }
        }

        [RelayCommand(CanExecute = nameof(CanLoadMore))]
        private async Task LoadMoreUsersAsync()
        {
            // Kiểm tra lại các cờ trạng thái
            if (_isLoadingMore || !_canLoadMore || IsBusy)
            {
                _logger.LogDebug("LoadMoreUsersAsync skipped. isLoadingMore: {IsLoadingMore}, canLoadMore: {CanLoadMore}, IsBusy: {IsBusy}", _isLoadingMore, _canLoadMore, IsBusy);
                return;
            }

            _isLoadingMore = true;
            _logger.LogDebug("LoadMoreUsers triggered.");
            await LoadUsersAsync(false);
        }

        private bool CanLoadMore() => _canLoadMore && !_isLoadingMore && !IsBusy;


        [RelayCommand]
        private async Task GoToUserDetailsAsync(Guid? userId)
        {
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                _logger.LogWarning("GoToUserDetailsAsync called with invalid UserId.");
                return;
            }

            try
            {
                _logger.LogInformation("Navigating to user details: {UserId}", userId.Value);
                await Shell.Current.GoToAsync($"{nameof(AdminUserDetailsPage)}?UserId={userId.Value}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Navigation failed for user {UserId}", userId.Value);
                await DisplayAlertAsync("Error", "Could not navigate to user details.", "OK");
            }
        }

        public void OnAppearing()
        {
            if (!Users.Any() && _initialLoadPending)
            {
                _logger.LogInformation("OnAppearing: Initial load triggered.");
                LoadUsersCommand.Execute(false);
            }
            else
            {
                _logger.LogInformation("OnAppearing: Skipping initial load.");
            }
        }
    }
}