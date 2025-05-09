
using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler (Admin)
    public interface IAdminPromotionApi
    {
        [Get("/admin/promotions")]
        Task<ApiResponse<IEnumerable<PromotionDto>>> GetAllPromotions();

        [Get("/admin/promotions/{id}")]
        Task<ApiResponse<PromotionDto>> GetPromotionById(Guid id);

        [Post("/admin/promotions")]
        Task<ApiResponse<PromotionDto>> CreatePromotion([Body] CreatePromotionDto dto);

        [Put("/admin/promotions/{id}")]
        Task<ApiResponse<object>> UpdatePromotion(Guid id, [Body] UpdatePromotionDto dto);

        [Delete("/admin/promotions/{id}")]
        Task<ApiResponse<object>> DeletePromotion(Guid id);
    }
}