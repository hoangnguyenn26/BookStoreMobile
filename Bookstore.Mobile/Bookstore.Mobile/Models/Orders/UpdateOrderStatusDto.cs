using Bookstore.Mobile.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class UpdateOrderStatusDto
    {
        [Required]
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus NewStatus { get; set; }
    }
}