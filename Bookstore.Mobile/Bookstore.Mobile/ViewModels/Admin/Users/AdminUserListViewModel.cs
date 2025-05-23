﻿using Bookstore.Mobile.Interfaces.Apis;
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

        private int _currentPage = 1;
        private const int PageSize = 20;
        private bool _initialLoadPending = true;

        public ObservableCollection<string> AvailableRoles { get; } = new() { "All", "Admin", "Staff", "User" };
        public ObservableCollection<string> AvailableStatuses { get; } = new() { "All", "Active", "Inactive" };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasFiltersApplied))]
        private string _selectedRoleFilter = "All";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasFiltersApplied))]
        private string _selectedStatusFilter = "All";

        [ObservableProperty]
        private ObservableCollection<UserDto> _users = new();

        [ObservableProperty]
        private string? _searchTerm;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadMoreUsersCommand))]
        private bool _canLoadMore = true;

        [ObservableProperty]
        private bool _isRefreshing; // Added for RefreshView

        public bool HasFiltersApplied => SelectedRoleFilter != "All" || SelectedStatusFilter != "All";

        public AdminUserListViewModel(
            IAdminUserApi userApi,
            ILogger<AdminUserListViewModel> logger)
        {
            _userApi = userApi ?? throw new ArgumentNullException(nameof(userApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Title = "Admin Users";
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
                await LoadUsers(true);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Filter change debounced/cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debounced filter change.");
                ErrorMessage = "Error applying filters. Please try again.";
            }
        }

        [RelayCommand]
        private async Task ClearFilters()
        {
            if (HasFiltersApplied)
            {
                SelectedRoleFilter = "All";
                SelectedStatusFilter = "All";
                await LoadUsers(true);
            }
        }

        [RelayCommand]
        private async Task LoadUsers(bool isRefreshing = false)
        {
            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Loading users (Page: {Page}, Refreshing: {IsRefreshing})", _currentPage, isRefreshing);

                if (isRefreshing)
                {
                    _currentPage = 1;
                    Users.Clear();
                    CanLoadMore = true;
                }

                bool? statusFilter = SelectedStatusFilter switch
                {
                    "Active" => true,
                    "Inactive" => false,
                    _ => null
                };

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
                    CanLoadMore = response.Content.Count() == PageSize;
                    _currentPage++;
                    _logger.LogInformation("Successfully loaded {UserCount} users", response.Content.Count());
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load users.";
                    _logger.LogError("Failed to load users. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, response.Error?.Content);
                }
            }, isRefreshing, nameof(ShowContent)); // Use isRefreshing for showBusy
        }

        [RelayCommand(CanExecute = nameof(CanExecuteLoadMore))]
        private async Task LoadMoreUsers()
        {
            await LoadUsers();
        }

        private bool CanExecuteLoadMore() => CanLoadMore && !IsBusy;

        [RelayCommand]
        private async Task GoToUserDetails(UserDto user)
        {
            if (user == null || user.Id == Guid.Empty) return;

            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Navigating to user details for ID: {UserId}", user.Id);
                await Shell.Current.GoToAsync($"{nameof(AdminUserDetailsPage)}?UserId={user.Id}");
            }, showBusy: false);
        }

        public async void OnAppearing()
        {
            if (_initialLoadPending)
            {
                _initialLoadPending = false;
                try
                {
                    await LoadUsers(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during initial user load");
                    ErrorMessage = "Failed to load users on startup.";
                }
            }
        }
    }
}