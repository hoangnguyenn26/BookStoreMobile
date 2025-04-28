
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class CreateOrderRequestDto
    {

        [Required]
        public Guid ShippingAddressId { get; set; }

        public string? PromotionCode { get; set; }

    }
}