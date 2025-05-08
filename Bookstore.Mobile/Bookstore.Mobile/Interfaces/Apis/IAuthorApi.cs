using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IAuthorApi
    {
        [Get("/v1/authors")]
        Task<ApiResponse<IEnumerable<AuthorDto>>> GetAuthors([Query] int page = 1, [Query] int pageSize = 10);

        [Get("/v1/authors/{id}")]
        Task<ApiResponse<AuthorDto>> GetAuthorById(Guid id);

        [Post("/admin/authors")]
        Task<ApiResponse<AuthorDto>> CreateAuthor([Body] CreateAuthorDto dto);

        [Put("/admin/authors/{id}")]
        Task<ApiResponse<object>> UpdateAuthor(Guid id, [Body] UpdateAuthorDto dto);

        [Delete("/admin/authors/{id}")]
        Task<ApiResponse<object>> DeleteAuthor(Guid id);
    }
}
