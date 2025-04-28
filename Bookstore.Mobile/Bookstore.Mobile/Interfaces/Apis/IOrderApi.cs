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
    }
}