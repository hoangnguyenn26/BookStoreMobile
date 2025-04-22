
using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IAuthApi
    {
        [Post("/auth/register")]
        Task<ApiResponse<UserDto>> Register([Body] RegisterRequestDto registerDto);

        [Post("/auth/login")]
        Task<ApiResponse<LoginResponseDto>> Login([Body] LoginRequestDto loginDto);
    }
}