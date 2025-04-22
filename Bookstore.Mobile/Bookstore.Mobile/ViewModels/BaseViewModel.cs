
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bookstore.Mobile.ViewModels
{
    // Kế thừa ObservableObject để tự động thông báo thay đổi thuộc tính
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string? _title;

        public bool IsNotBusy => !IsBusy;

        //Hàm xử lý lỗi chung
        protected virtual async Task DisplayAlertAsync(string title, string message, string cancel = "OK")
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }

        // (Optional) Hàm điều hướng chung (sẽ cần inject INavigationService sau)
        // protected readonly INavigationService _navigationService;
        // public BaseViewModel(INavigationService navigationService) { _navigationService = navigationService; }
    }
}