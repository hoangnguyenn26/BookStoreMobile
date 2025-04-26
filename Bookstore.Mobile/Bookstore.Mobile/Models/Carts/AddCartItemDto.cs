
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class AddCartItemDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }
}