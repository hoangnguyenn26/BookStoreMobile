using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminPromotionListViewModel : BaseViewModel
    {
        private readonly IAdminPromotionApi _promotionApi;
        private readonly ILogger<AdminPromotionListViewModel> _logger;

        public AdminPromotionListViewModel(IAdminPromotionApi promotionApi, ILogger<AdminPromotionListViewModel> logger)
        {
            _promotionApi = promotionApi;
            _logger = logger;
            Title = "Manage Promotions";
            Promotions = new ObservableCollection<PromotionDto>();
        }

        [ObservableProperty] private ObservableCollection<PromotionDto> _promotions;

        [RelayCommand]
        private async Task LoadPromotionsAsync(object? parameter)
        {
            bool isRefreshing = parameter is bool b && b;
            if (IsBusy && !isRefreshing) return;
            IsBusy = true;
            if (isRefreshing) Promotions.Clear();
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Loading all promotions for admin.");
                var response = await _promotionApi.GetAllPromotions();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (!isRefreshing) Promotions.Clear(); // Clear if not refreshing (initial load)
                    foreach (var promo in response.Content.OrderByDescending(p => p.CreatedAtUtc))
                    {
                        Promotions.Add(promo);
                    }
                    _logger.LogInformation("Loaded {Count} promotions.", Promotions.Count);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load promotions.";
                    _logger.LogWarning("Failed to load promotions. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex) { ErrorMessage = ex.Message; _logger.LogError(ex, "Error loading promotions."); }
            finally { IsBusy = false; OnPropertyChanged(nameof(ShowContent)); }
        }

        [RelayCommand] private async Task GoToAddPromotionAsync() => await Shell.Current.GoToAsync($"{nameof(AddEditPromotionPage)}?PromotionId={Guid.Empty}");
        [RelayCommand] private async Task GoToEditPromotionAsync(Guid? promotionId) { if (promotionId.HasValue) await Shell.Current.GoToAsync($"{nameof(AddEditPromotionPage)}?PromotionId={promotionId.Value}"); }

        [RelayCommand]
        private async Task DeletePromotionAsync(Guid? promotionId)
        {
            if (!promotionId.HasValue || IsBusy) return;
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Delete this promotion?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                var response = await _promotionApi.DeletePromotion(promotionId.Value);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Promotion {PromotionId} deleted.", promotionId.Value);
                    await LoadPromotionsAsync(true); // Reload list
                }
                else
                {
                    string error = response.Error?.Content ?? "Failed to delete";
                    await DisplayAlertAsync("Error", error);
                    _logger.LogWarning("Failed to delete promotion {PromotionId}. Status: {StatusCode}", promotionId.Value, response.StatusCode);
                }
            }
            catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message); _logger.LogError(ex, "Error deleting promotion {PromotionId}", promotionId.Value); }
            finally { IsBusy = false; }
        }
        public void OnAppearing() { if (Promotions.Count == 0 || IsRefreshNeeded) LoadPromotionsCommand.Execute(true); IsRefreshNeeded = false; }
        public bool IsRefreshNeeded { get; set; } = true; // Cờ để load lại khi quay về
    }
}