using Refit;

namespace Bookstore.Mobile.Models
{
    public interface IWishlistApi
    {
        [Get("/v1/wishlist")]
        Task<ApiResponse<IEnumerable<WishlistItemDto>>> GetWishlist();

        [Post("/v1/wishlist/{bookId}")]
        Task<ApiResponse<object>> AddToWishlist(Guid bookId);

        [Delete("/v1/wishlist/{bookId}")]
        Task<ApiResponse<object>> RemoveFromWishlist(Guid bookId);
    }
}
