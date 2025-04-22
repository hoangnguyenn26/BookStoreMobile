
using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IBooksApi
    {
        [Get("/v1/books")]
        Task<ApiResponse<IEnumerable<BookDto>>> GetBooks(
            [Query] Guid? categoryId,
            [Query] string? search,
            [Query] int page = 1,
            [Query] int pageSize = 10,
            [Header("Authorization")] string? authorization = null);

        [Get("/v1/books/{id}")]
        Task<ApiResponse<BookDto>> GetBookById(Guid id, [Header("Authorization")] string? authorization = null);

    }
}