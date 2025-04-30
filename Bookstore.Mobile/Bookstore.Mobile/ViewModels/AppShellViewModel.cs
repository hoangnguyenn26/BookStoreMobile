using Bookstore.Mobile.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AppShellViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        public AppShellViewModel(IAuthService authService)
        {
            _authService = authService;
            _authService.AuthStateChanged += OnAuthStateChanged;
            UpdateMenuVisibility();
        }

        // Property để binding IsVisible cho các mục menu Admin/Staff
        [ObservableProperty]
        private bool _isAdminOrStaff;

        [ObservableProperty]
        private bool _isAdmin;
        private void OnAuthStateChanged(object? sender, EventArgs e)
        {
            UpdateMenuVisibility();
        }

        private void UpdateMenuVisibility()
        {
            // Kiểm tra vai trò bằng AuthService
            bool staffCheck = _authService.HasRole("Staff");
            bool adminCheck = _authService.HasRole("Admin");

            IsAdmin = adminCheck;
            IsAdminOrStaff = staffCheck || adminCheck;
        }
    }
}