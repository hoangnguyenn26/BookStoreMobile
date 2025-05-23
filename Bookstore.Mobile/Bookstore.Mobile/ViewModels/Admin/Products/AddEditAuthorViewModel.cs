﻿using Bookstore.Mobile.Helpers;
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;

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
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveAuthorCommand))]
        private string? _name;

        [ObservableProperty]
        private string? _biography;

        public bool CanSaveAuthorPublic => CanSaveAuthor();

        public string? AuthorIdString
        {
            get => _authorIdString;
            set
            {
                if (_authorIdString != value)
                {
                    _authorIdString = value;
                    ProcessAuthorId(value);
                }
            }
        }

        private async void ProcessAuthorId(string? idString)
        {
            await RunSafeAsync(async () =>
            {
                if (Guid.TryParse(idString, out Guid parsedId) && parsedId != Guid.Empty)
                {
                    _actualAuthorId = parsedId;
                    Title = "Edit Author";
                    await LoadAuthorDetailsAsync();
                }
                else
                {
                    _actualAuthorId = Guid.Empty;
                    Title = "Add New Author";
                    ResetForm();
                }
            }, nameof(ShowContent));
        }

        private void ResetForm()
        {
            Name = Biography = null;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Biography));
        }

        [RelayCommand]
        private async Task LoadAuthorDetailsAsync()
        {
            await RunSafeAsync(async () =>
            {
                if (_actualAuthorId == Guid.Empty)
                {
                    _logger.LogWarning("Attempted to load author details with empty ID");
                    return;
                }

                _logger.LogInformation("Fetching author details for ID: {AuthorId}", _actualAuthorId);
                var response = await _authorApi.GetAuthorById(_actualAuthorId);
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Name = response.Content.Name;
                    Biography = response.Content.Biography;
                    _logger.LogInformation("Successfully loaded author details for ID: {AuthorId}", _actualAuthorId);
                }
                else
                {
                    ErrorMessage = response.Error?.Content ?? "Failed to load author details.";
                    _logger.LogWarning("Failed to load author details for ID: {AuthorId}. Error: {Error}", _actualAuthorId, ErrorMessage);
                }
            }, showBusy: false, nameof(ShowContent)); // Set showBusy to false to bypass IsBusy check
        }

        private bool CanSaveAuthor() => !string.IsNullOrWhiteSpace(Name) && IsNotBusy;

        [RelayCommand(CanExecute = nameof(CanSaveAuthor))]
        private async Task SaveAuthorAsync()
        {
            await RunSafeAsync(async () =>
            {
                _logger.LogInformation("Attempting to save author (ID: {AuthorId})", _actualAuthorId);

                bool success = false;
                ApiResponse<object>? response = null;
                ApiResponse<AuthorDto>? createResponse = null;

                if (_actualAuthorId == Guid.Empty) // Add
                {
                    var createDto = new CreateAuthorDto { Name = Name!, Biography = Biography };
                    createResponse = await _authorApi.CreateAuthor(createDto);
                    success = createResponse.IsSuccessStatusCode;
                    if (!success)
                        ErrorMessage = ErrorMessageHelper.ToFriendlyErrorMessage(createResponse.Error?.Content) ?? "Failed to create author.";
                }
                else // Update
                {
                    var updateDto = new UpdateAuthorDto { Name = Name!, Biography = Biography };
                    response = await _authorApi.UpdateAuthor(_actualAuthorId, updateDto);
                    success = response.IsSuccessStatusCode;
                    if (!success)
                        ErrorMessage = ErrorMessageHelper.ToFriendlyErrorMessage(response.Error?.Content) ?? "Failed to update author.";
                }

                if (success)
                {
                    _logger.LogInformation("Author saved successfully (ID: {AuthorId})", _actualAuthorId == Guid.Empty ? (createResponse?.Content?.Id.ToString() ?? "(New)") : _actualAuthorId.ToString());
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    _logger.LogWarning("Failed to save author (ID: {AuthorId}). Reason: {Reason}", _actualAuthorId, ErrorMessage);
                    await DisplayAlertAsync("Save Failed", ErrorMessage ?? "Could not save the author.");
                }
            }, nameof(ShowContent));
        }
    }
}