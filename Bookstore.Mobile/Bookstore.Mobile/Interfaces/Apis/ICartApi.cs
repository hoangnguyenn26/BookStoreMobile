using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler
    public interface ICartApi
    {
        [Get("/v1/cart")]
        Task<ApiResponse<IEnumerable<CartItemDto>>> GetCart();

        [Post("/v1/cart/items")]
        Task<ApiResponse<CartItemDto>> AddOrUpdateItem([Body] AddCartItemDto item);

        [Put("/v1/cart/items/{bookId}")]
        Task<ApiResponse<object>> UpdateItemQuantity(Guid bookId, [Body] UpdateCartItemDto item);

        [Delete("/v1/cart/items/{bookId}")]
        Task<ApiResponse<object>> RemoveItem(Guid bookId);

        [Delete("/v1/cart")]
        Task<ApiResponse<object>> ClearCart();
    }
}