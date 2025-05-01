using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface ICategoriesApi
    {
        [Get("/v1/categories")]
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategories();

        [Post("/admin/categories")]
        Task<ApiResponse<CategoryDto>> CreateCategory([Body] CreateCategoryDto dto);

        [Put("/admin/categories/{id}")]
        Task<ApiResponse<object>> UpdateCategory(Guid id, [Body] UpdateCategoryDto dto);

        [Delete("/admin/categories/{id}")]
        Task<ApiResponse<object>> DeleteCategory(Guid id);
    }
}