using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface ICategoriesApi
    {
        [Get("/v1/categories")]
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategories();

        [Get("/v1/categories/{id}")]
        Task<ApiResponse<CategoryDto>> GetCategoryById(Guid id);

        [Post("/v1/categories")]
        Task<ApiResponse<CategoryDto>> CreateCategory([Body] CreateCategoryDto dto);

        [Put("/v1/categories/{id}")]
        Task<ApiResponse<object>> UpdateCategory(Guid id, [Body] UpdateCategoryDto dto);

        [Delete("/v1/categories/{id}")]
        Task<ApiResponse<object>> DeleteCategory(Guid id);
    }
}