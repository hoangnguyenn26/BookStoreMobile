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
            // Đăng ký lắng nghe sự kiện thay đổi trạng thái Auth
            _authService.AuthStateChanged += OnAuthStateChanged;
            // Cập nhật trạng thái ban đầu
            UpdateAdminStaffVisibility();
        }

        // Property để binding IsVisible cho các mục menu Admin/Staff
        [ObservableProperty]
        private bool _isAdminOrStaff;

        private void OnAuthStateChanged(object? sender, EventArgs e)
        {
            // Cập nhật trạng thái hiển thị khi trạng thái đăng nhập thay đổi
            UpdateAdminStaffVisibility();
        }

        private void UpdateAdminStaffVisibility()
        {
            // Kiểm tra xem user có phải Admin hoặc Staff không
            IsAdminOrStaff = _authService.HasRole("Admin") || _authService.HasRole("Staff");
        }
    }
}