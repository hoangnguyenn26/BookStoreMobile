namespace Bookstore.Mobile.Models
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } = null!;
    }
}