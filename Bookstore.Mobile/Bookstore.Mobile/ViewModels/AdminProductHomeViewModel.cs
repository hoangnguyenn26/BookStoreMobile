using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminProductHomeViewModel : BaseViewModel
    {
        public override bool ShowContent => !IsBusy && !HasError;

        public AdminProductHomeViewModel()
        {
            Title = "Product Home";
        }

        // Example async operation using RunSafeAsync
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async Task LoadDataAsync()
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
