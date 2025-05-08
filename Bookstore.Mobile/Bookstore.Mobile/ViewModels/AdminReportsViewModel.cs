namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminReportsViewModel : BaseViewModel
    {
        public override bool ShowContent => !IsBusy && !HasError;

        public AdminReportsViewModel()
        {
            Title = "Reports";
        }

        // Example async operation using RunSafeAsync
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async Task LoadReportsAsync()
        {
            await RunSafeAsync(async () =>
            {
                // Simulate data loading
                await Task.Delay(500);
                // Add your data loading logic here
            }, nameof(ShowContent));
        }
    }
}
