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

        // Paging State
        private int _currentPage = 1;
        private const int PageSize = 20;
        private bool _canLoadMore = true;
        private bool _initialLoadPending = true;

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
        private string? _searchTerm;

        public override bool ShowContent => !IsBusy && !HasError;
        public bool HasFiltersApplied => SelectedRoleFilter != "All" || SelectedStatusFilter != "All";

        public AdminUserListViewModel(IAdminUserApi userApi, ILogger<AdminUserListViewModel> logger)
        {
            _userApi = userApi ?? throw new ArgumentNullException(nameof(userApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Title = "Admin Users";
            Users = new ObservableCollection<UserDto>();
            AvailableRoles = new ObservableCollection<string> { "All", "Admin", "Staff", "User" };
            AvailableStatuses = new ObservableCollection<string> { "All", "Active", "Inactive" };
        }

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
                SelectedRoleFilter = "All";
                SelectedStatusFilter = "All";
            }
        }

        [RelayCommand]
        private async Task LoadUsersAsync(bool isRefreshing = false)
        {
            await RunSafeAsync(async () =>
            {
                if (isRefreshing)
                {
                    _currentPage = 1;
                    Users.Clear();
                    _canLoadMore = true;
                }
                bool? statusFilter = null;
                if (SelectedStatusFilter == "Active")
                    statusFilter = true;
                else if (SelectedStatusFilter == "Inactive")
                    statusFilter = false;

                var response = await _userApi.GetUsers(
                    _currentPage,
                    PageSize,
                    SelectedRoleFilter != "All" ? SelectedRoleFilter : null,
                    statusFilter);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    foreach (var user in response.Content)
                    {
                        Users.Add(user);
                    }
                    _canLoadMore = response.Content.Count() == PageSize;
                    _currentPage++;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load users.";
                }
            }, showBusy: true);
        }

        [RelayCommand(CanExecute = nameof(CanLoadMore))]
        private async Task LoadMoreUsersAsync()
        {
            if (!CanLoadMore()) return;
            await LoadUsersAsync(false);
        }

        private bool CanLoadMore() => _canLoadMore && !IsBusy;

        [RelayCommand]
        private async Task GoToUserDetailsAsync(Guid? userId)
        {
            if (!userId.HasValue || userId.Value == Guid.Empty) return;

            await RunSafeAsync(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(AdminUserDetailsPage)}?UserId={userId.Value}");
            }, showBusy: false);
        }

        public async void OnAppearing()
        {
            if (_initialLoadPending)
            {
                _initialLoadPending = false;
                await LoadUsersCommand.ExecuteAsync(false);
            }
        }
    }
}