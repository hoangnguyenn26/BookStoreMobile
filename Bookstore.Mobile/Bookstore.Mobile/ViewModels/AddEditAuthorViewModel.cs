using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(AuthorIdString), "AuthorId")]
    public partial class AddEditAuthorViewModel : BaseViewModel
    {
        private readonly IAuthorApi _authorApi;
        private readonly ILogger<AddEditAuthorViewModel> _logger;

        private Guid _actualAuthorId = Guid.Empty;
        private string? _authorIdString;

        public AddEditAuthorViewModel(IAuthorApi authorApi, ILogger<AddEditAuthorViewModel> logger)
        {
            _authorApi = authorApi;
            _logger = logger;
            // Title sẽ được đặt trong ProcessAuthorId
        }

        // Properties cho Binding
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveAuthorCommand))] private string? _name;
        [ObservableProperty] private string? _biography;

        public override bool ShowContent => !IsBusy && !HasError;
        public bool CanSaveAuthorPublic => CanSaveAuthor();

        public string? AuthorIdString
        {
            get => _authorIdString;
            set { if (_authorIdString != value) { _authorIdString = value; ProcessAuthorId(value); } }
        }

        private async void ProcessAuthorId(string? idString)
        {
            IsBusy = true; ErrorMessage = null;
            if (Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
            {
                _actualAuthorId = parsedId; Title = "Edit Author";
                await LoadAuthorDetailsAsync();
            }
            else
            {
                _actualAuthorId = Guid.Empty; Title = "Add New Author";
                ResetForm();
            }
            IsBusy = false; OnPropertyChanged(nameof(ShowContent));
        }

        private void ResetForm() { Name = Biography = null; }

        [RelayCommand]
        private async Task LoadAuthorDetailsAsync()
        {
            await RunSafeAsync(async () =>
            {
                if (_actualAuthorId == Guid.Empty) return;
                var response = await _authorApi.GetAuthorById(_actualAuthorId);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Name = response.Content.Name;
                    Biography = response.Content.Biography;
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load author details.";
                }
            }, nameof(ShowContent));
        }

        // Điều kiện để Save
        private bool CanSaveAuthor() => !string.IsNullOrWhiteSpace(Name) && IsNotBusy;

        // Lệnh Save
        [RelayCommand(CanExecute = nameof(CanSaveAuthor))]
        private async Task SaveAuthorAsync()
        {
            IsBusy = true; ErrorMessage = null;
            _logger.LogInformation("Attempting to save author (Actual Id: {AuthorId})", _actualAuthorId);

            try
            {
                bool success = false;
                ApiResponse<object>? response = null;
                ApiResponse<AuthorDto>? createResponse = null;

                if (_actualAuthorId == Guid.Empty) // Add
                {
                    var createDto = new CreateAuthorDto { Name = Name!, Biography = Biography };
                    createResponse = await _authorApi.CreateAuthor(createDto);
                    success = createResponse.IsSuccessStatusCode;
                    if (!success) ErrorMessage = createResponse.Error?.Content ?? "Failed";
                }
                else // Update
                {
                    var updateDto = new UpdateAuthorDto { Name = Name!, Biography = Biography };
                    response = await _authorApi.UpdateAuthor(_actualAuthorId, updateDto);
                    success = response.IsSuccessStatusCode;
                    if (!success) ErrorMessage = response.Error?.Content ?? "Failed";
                }

                if (success)
                {
                    _logger.LogInformation("Author saved successfully (Id: {AuthorId})", _actualAuthorId == Guid.Empty ? (createResponse?.Content?.Id.ToString() ?? "(New)") : _actualAuthorId.ToString());
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    _logger.LogWarning("Failed to save author {AuthorId}. Reason: {Reason}", _actualAuthorId, ErrorMessage);
                    await DisplayAlertAsync("Save Failed", ErrorMessage ?? "Could not save the author.");
                }
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error saving author {AuthorId}", _actualAuthorId);
                ErrorMessage = ex.Message;
                await DisplayAlertAsync("Validation Error", ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while saving author {AuthorId}", _actualAuthorId);
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await DisplayAlertAsync("Error", ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}