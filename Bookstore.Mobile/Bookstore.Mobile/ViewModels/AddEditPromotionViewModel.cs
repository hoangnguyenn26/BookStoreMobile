// Bookstore.Mobile/ViewModels/AddEditPromotionViewModel.cs
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(PromotionIdString), "PromotionId")]
    public partial class AddEditPromotionViewModel : BaseViewModel
    {
        private readonly IAdminPromotionApi _promotionApi;
        private readonly ILogger<AddEditPromotionViewModel> _logger;

        private Guid _actualPromotionId = Guid.Empty;
        private string? _promotionIdString;

        public AddEditPromotionViewModel(IAdminPromotionApi promotionApi, ILogger<AddEditPromotionViewModel> logger)
        {
            _promotionApi = promotionApi ?? throw new ArgumentNullException(nameof(promotionApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hasEndDate = false;
            _endDateForPicker = DateTime.Now.Date.AddMonths(1);
        }

        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private string? _code;
        [ObservableProperty] private string? _description;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private string? _discountPercentage;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private string? _discountAmount;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private bool _isPercentageDiscount = true;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private bool _isFixedAmountDiscount = false;

        partial void OnIsPercentageDiscountChanged(bool value)
        {
            if (value) IsFixedAmountDiscount = false;
            SavePromotionCommand.NotifyCanExecuteChanged();
        }
        partial void OnIsFixedAmountDiscountChanged(bool value)
        {
            if (value) IsPercentageDiscount = false;
            SavePromotionCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private DateTime _startDate = DateTime.Now.Date;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private DateTime _endDateForPicker;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private bool _hasEndDate;

        public DateTime? ActualEndDateToSave => HasEndDate ? EndDateForPicker.Date : null; // Chỉ lấy phần Date

        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SavePromotionCommand))] private string? _maxUsage;
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private string? _errorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool ShowFormContent => !IsBusy && !HasError;

        public string? PromotionIdString
        {
            get => _promotionIdString;
            set { if (_promotionIdString != value) { _promotionIdString = value; ProcessPromotionId(value); } }
        }

        private async void ProcessPromotionId(string? idString)
        {
            IsBusy = true;
            ErrorMessage = null;
            try
            {
                if (Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
                {
                    _actualPromotionId = parsedId;
                    Title = "Edit Promotion";
                    _logger.LogInformation("Loading promotion details for Id: {PromotionId}", _actualPromotionId);
                    await LoadPromotionDetailsAsync(_actualPromotionId);
                }
                else
                {
                    _actualPromotionId = Guid.Empty;
                    Title = "Add New Promotion";
                    ResetForm();
                    _logger.LogInformation("Processing as 'Add New Promotion'.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PromotionId {PromotionIdString}", idString);
                ErrorMessage = "Failed to process promotion ID.";
                OnPropertyChanged(nameof(HasError));
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowFormContent));
            }
        }

        private void ResetForm()
        {
            Code = null;
            Description = null;
            IsPercentageDiscount = true;
            DiscountPercentage = null;
            DiscountAmount = null;
            StartDate = DateTime.Now.Date;
            HasEndDate = false;
            EndDateForPicker = DateTime.Now.Date.AddMonths(1);
            MaxUsage = null;
            IsActive = true;
            _logger.LogInformation("Form reset for new promotion.");
        }

        private async Task LoadPromotionDetailsAsync(Guid promoId)
        {
            try
            {
                var response = await _promotionApi.GetPromotionById(promoId);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var dto = response.Content;
                    Code = dto.Code;
                    Description = dto.Description;
                    IsPercentageDiscount = dto.DiscountPercentage.HasValue;
                    IsFixedAmountDiscount = dto.DiscountAmount.HasValue;
                    DiscountPercentage = dto.DiscountPercentage?.ToString("F2");
                    DiscountAmount = dto.DiscountAmount?.ToString("F0"); // Giả sử không có số lẻ cho tiền
                    StartDate = dto.StartDate.Date;
                    if (dto.EndDate.HasValue)
                    {
                        HasEndDate = true;
                        EndDateForPicker = dto.EndDate.Value.Date;
                    }
                    else
                    {
                        HasEndDate = false;
                        EndDateForPicker = DateTime.Now.Date.AddMonths(1); // Mặc định khi bật lại
                    }
                    MaxUsage = dto.MaxUsage?.ToString();
                    IsActive = dto.IsActive;
                    _logger.LogInformation("Promotion details loaded successfully for {PromotionId}", promoId);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load promotion details.";
                    _logger.LogWarning("Failed to load promotion {PromotionId} for editing. Status: {StatusCode}", promoId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading promotion details for {PromotionId}", promoId);
                ErrorMessage = "An error occurred while loading promotion details.";
            }
        }

        private bool CanSavePromotion()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(Code)) return false;
            if (IsPercentageDiscount)
            {
                if (string.IsNullOrWhiteSpace(DiscountPercentage) || !decimal.TryParse(DiscountPercentage, out var dp) || dp <= 0 || dp > 100) return false;
            }
            else if (IsFixedAmountDiscount)
            {
                if (string.IsNullOrWhiteSpace(DiscountAmount) || !decimal.TryParse(DiscountAmount, out var da) || da <= 0) return false;
            }
            else
            {
                return false; // Phải chọn một loại giảm giá
            }

            if (MaxUsage != null && (!int.TryParse(MaxUsage, out int mu) || mu < 0)) return false;
            if (HasEndDate && EndDateForPicker.Date < StartDate.Date) return false;

            return true;
        }

        [RelayCommand(CanExecute = nameof(CanSavePromotion))]
        private async Task SavePromotionAsync()
        {
            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Attempting to save promotion (Id: {PromotionId})", _actualPromotionId);

            try
            {
                bool success = false;
                ApiResponse<object>? updateResponse = null;
                ApiResponse<PromotionDto>? createResponse = null;

                decimal? dpValue = IsPercentageDiscount && decimal.TryParse(DiscountPercentage, out var p) ? p : null;
                decimal? daValue = IsFixedAmountDiscount && decimal.TryParse(DiscountAmount, out var a) ? a : null;
                int? muValue = int.TryParse(MaxUsage, out int m) ? m : null;
                DateTime? finalEndDate = ActualEndDateToSave;

                if (_actualPromotionId == Guid.Empty)
                {
                    var createDto = new CreatePromotionDto
                    {
                        Code = Code!,
                        Description = Description,
                        DiscountPercentage = dpValue,
                        DiscountAmount = daValue,
                        StartDate = StartDate.Date,
                        EndDate = finalEndDate,
                        MaxUsage = muValue,
                        IsActive = IsActive
                    };
                    createResponse = await _promotionApi.CreatePromotion(createDto);
                    success = createResponse.IsSuccessStatusCode;
                    if (!success) ErrorMessage = createResponse.Error?.Content ?? "Failed to create promotion";
                }
                else
                {
                    var updateDto = new UpdatePromotionDto
                    {
                        Description = Description,
                        DiscountPercentage = dpValue,
                        DiscountAmount = daValue,
                        StartDate = StartDate.Date,
                        EndDate = finalEndDate,
                        MaxUsage = muValue,
                        IsActive = IsActive
                    };
                    updateResponse = await _promotionApi.UpdatePromotion(_actualPromotionId, updateDto);
                    success = updateResponse.IsSuccessStatusCode;
                    if (!success) ErrorMessage = updateResponse.Error?.Content ?? "Failed to update promotion";
                }

                if (success)
                {
                    _logger.LogInformation("Promotion saved successfully (Id: {PromotionId})", _actualPromotionId == Guid.Empty ? (createResponse?.Content?.Id.ToString() ?? "(New)") : _actualPromotionId.ToString());
                    // Thông báo cho trang list biết để load lại
                    if (Shell.Current.CurrentPage.BindingContext is AdminPromotionListViewModel listVm)
                    {
                        listVm.IsRefreshNeeded = true;
                    }
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    _logger.LogWarning("Failed to save promotion {PromotionId}. Reason: {Reason}", _actualPromotionId, ErrorMessage);
                    await DisplayAlertAsync("Save Failed", ErrorMessage ?? "Could not save the promotion.");
                }
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error saving promotion {PromotionId}", _actualPromotionId);
                ErrorMessage = ex.Message;
                await DisplayAlertAsync("Validation Error", ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while saving promotion {PromotionId}", _actualPromotionId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {

        }
    }
}