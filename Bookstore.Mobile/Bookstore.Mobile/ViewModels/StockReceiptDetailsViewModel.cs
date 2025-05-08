namespace Bookstore.Mobile.ViewModels
{
    public partial class StockReceiptDetailsViewModel : BaseViewModel
    {
        public override bool ShowContent => !IsBusy && !HasError;

        public StockReceiptDetailsViewModel()
        {
            Title = "Stock Receipt Details";
        }

        // Example async operation using RunSafeAsync
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async Task LoadReceiptDetailsAsync()
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
