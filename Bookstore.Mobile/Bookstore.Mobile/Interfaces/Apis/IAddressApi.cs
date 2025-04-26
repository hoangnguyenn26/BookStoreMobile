using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler
    public interface IAddressApi
    {
        [Get("/v1/user/addresses")]
        Task<ApiResponse<IEnumerable<AddressDto>>> GetAddresses();

        [Get("/v1/user/addresses/{addressId}")]
        Task<ApiResponse<AddressDto>> GetAddressById(Guid addressId);

        [Post("/v1/user/addresses")]
        Task<ApiResponse<AddressDto>> CreateAddress([Body] CreateAddressDto address);

        [Put("/v1/user/addresses/{addressId}")]
        Task<ApiResponse<object>> UpdateAddress(Guid addressId, [Body] UpdateAddressDto address);

        [Delete("/v1/user/addresses/{addressId}")]
        Task<ApiResponse<object>> DeleteAddress(Guid addressId);

        [Post("/v1/user/addresses/{addressId}/setdefault")]
        Task<ApiResponse<object>> SetDefaultAddress(Guid addressId);
    }
}