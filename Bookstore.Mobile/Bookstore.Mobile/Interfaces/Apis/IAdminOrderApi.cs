using Bookstore.Mobile.Enums;
using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IAdminOrderApi
    {
        [Get("/admin/orders")]
        Task<ApiResponse<IEnumerable<OrderSummaryDto>>> GetAllOrders(
            [Query] int page = 1,
            [Query] int pageSize = 10,
            [Query] OrderStatus? status = null);

        [Get("/admin/orders/{orderId}")]
        Task<ApiResponse<OrderDto>> GetOrderById(Guid orderId);

        [Put("/admin/orders/{orderId}/status")]
        Task<ApiResponse<object>> UpdateOrderStatus(Guid orderId, [Body] UpdateOrderStatusDto statusDto);
    }
}