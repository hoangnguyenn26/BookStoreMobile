namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminPromotionListViewModel : BaseViewModel
    {
        public override bool ShowContent => !IsBusy && !HasError;

        public AdminPromotionListViewModel()
        {
            Title = "Promotions";
        }

        // Example async operation using RunSafeAsync
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async Task LoadPromotionsAsync()
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
