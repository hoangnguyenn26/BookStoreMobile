using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class CreateAuthorDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Biography { get; set; }
    }
}