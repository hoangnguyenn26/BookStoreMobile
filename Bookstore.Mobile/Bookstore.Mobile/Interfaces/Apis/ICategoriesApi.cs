using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface ICategoriesApi
    {
        [Get("/v1/categories")]
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategories();
    }
}