// Bookstore.Mobile/Interfaces/Apis/IOrderApi.cs
using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler
    public interface IOrderApi
    {
        [Post("/v1/orders")]
        Task<ApiResponse<OrderDto>> CreateOnlineOrder([Body] CreateOrderRequestDto orderRequest);

        [Get("/v1/orders")]
        Task<ApiResponse<IEnumerable<OrderSummaryDto>>> GetMyOrders([Query] int page = 1, [Query] int pageSize = 10);

        [Get("/v1/orders/{orderId}")]
        Task<ApiResponse<OrderDto>> GetMyOrderById(Guid orderId);

        [Put("/v1/orders/{orderId}/cancel")]
        Task<ApiResponse<object>> CancelMyOrder(Guid orderId);

        // Admin endpoints
        [Get("/admin/orders")]
        Task<ApiResponse<IEnumerable<OrderSummaryDto>>> GetAllOrdersForAdmin(
            [Query] int page = 1,
            [Query] int pageSize = 10,
            [Query] object? status = null);

        [Get("/admin/orders/{orderId}")]
        Task<ApiResponse<OrderDto>> GetOrderByIdForAdmin(Guid orderId);

        [Put("/admin/orders/{orderId}/status")]
        Task<ApiResponse<object>> UpdateOrderStatus(Guid orderId, [Body] UpdateOrderStatusDto statusDto);

        [Post("/staff/orders")] // API Tạo đơn tại quầy
        Task<ApiResponse<OrderDto>> CreateInStoreOrder([Body] CreateInStoreOrderRequestDto orderRequest);
    }
}