// Bookstore.Mobile/Interfaces/Services/IAuthService.cs
using Bookstore.Mobile.Models;

namespace Bookstore.Mobile.Interfaces.Services
{
    public interface IAuthService
    {
        bool IsLoggedIn { get; }
        UserDto? CurrentUser { get; }
        string? AuthToken { get; }
        string? LastErrorMessage { get; }

        Task<bool> LoginAsync(LoginRequestDto loginRequest);
        Task<bool> RegisterAsync(RegisterRequestDto registerRequest);
        Task LogoutAsync();
        Task InitializeAsync();
    }
}