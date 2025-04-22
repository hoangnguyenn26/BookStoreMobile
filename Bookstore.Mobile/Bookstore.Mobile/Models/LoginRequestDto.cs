using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class LoginRequestDto
    {
        [Required]
        // Có thể là UserName hoặc Email
        public string LoginIdentifier { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}