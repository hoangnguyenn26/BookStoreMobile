using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IReviewApi
    {
        // Endpoint lấy review cho sách (thường là public)
        [Get("/v1/books/{bookId}/reviews")]
        Task<ApiResponse<IEnumerable<ReviewDto>>> GetBookReviews(Guid bookId, [Query] int page = 1, [Query] int pageSize = 5); // Lấy ít review ban đầu

        // Endpoint gửi review (cần Auth)
        [Post("/v1/books/{bookId}/reviews")]
        Task<ApiResponse<ReviewDto>> SubmitReview(Guid bookId, [Body] CreateReviewDto reviewDto);
    }
}