using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Bookstore.Mobile.ViewModels
{
    [QueryProperty(nameof(BookId), "BookId")]
    [QueryProperty(nameof(BookTitle), "BookTitle")]
    public partial class SubmitReviewViewModel : BaseViewModel
    {
        private readonly IReviewApi _reviewApi;
        private readonly ILogger<SubmitReviewViewModel> _logger;
        // private readonly INavigationService _navigationService;

        public SubmitReviewViewModel(IReviewApi reviewApi, ILogger<SubmitReviewViewModel> logger)
        {
            _reviewApi = reviewApi;
            _logger = logger;
        }

        [ObservableProperty]
        private Guid _bookId;

        [ObservableProperty]
        private string? _bookTitle;

        partial void OnBookTitleChanged(string? value)
        {
            Title = $"Review '{value ?? "Book"}'";
        }


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentRating))] // Thông báo thay đổi cho CurrentRating để cập nhật sao
        [NotifyCanExecuteChangedFor(nameof(SubmitReviewCommand))]
        private int _selectedRating = 0; // Lưu rating người dùng chọn

        [ObservableProperty]
        private string? _comment;

        public List<int> RatingOptions { get; } = new List<int> { 1, 2, 3, 4, 5 };

        public int CurrentRating => SelectedRating;

        public bool CanSubmitReview => SelectedRating > 0 && IsNotBusy;


        // --- Commands ---
        [RelayCommand]
        private void SetRating(object? ratingParam) // Nhận int từ CommandParameter
        {
            if (ratingParam is int rating && rating >= 1 && rating <= 5)
            {
                SelectedRating = rating; // Cập nhật rating được chọn
                _logger.LogInformation("Rating set to: {Rating}", rating);
            }
        }


        [RelayCommand(CanExecute = nameof(CanSubmitReview))]
        private async Task SubmitReviewAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            _logger.LogInformation("Submitting review for Book {BookId} with Rating {Rating}", BookId, SelectedRating);

            try
            {
                var createDto = new CreateReviewDto
                {
                    Rating = SelectedRating,
                    Comment = Comment
                };

                var response = await _reviewApi.SubmitReview(BookId, createDto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Review submitted successfully for Book {BookId}", BookId);
                    // Thành công, quay lại trang chi tiết sách
                    await DisplayAlertAsync("Review Submitted", "Thank you for your feedback!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed to submit review.";
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && errorContent.Contains("already reviewed"))
                    {
                        ErrorMessage = "You have already reviewed this book.";
                    }
                    else
                    {
                        ErrorMessage = $"Error: {errorContent}";
                    }
                    _logger.LogWarning("Failed to submit review for Book {BookId}. Status: {StatusCode}, Reason: {Reason}", BookId, response.StatusCode, ErrorMessage);
                    await DisplayAlertAsync("Submission Failed", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while submitting review for Book {BookId}", BookId);
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