
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class UpdateCartItemDto
    {
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}