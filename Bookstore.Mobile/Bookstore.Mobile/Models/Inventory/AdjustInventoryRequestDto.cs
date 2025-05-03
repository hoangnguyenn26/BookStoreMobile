using Bookstore.Mobile.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class AdjustInventoryRequestDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        public int ChangeQuantity { get; set; }

        [Required]
        [EnumDataType(typeof(InventoryReason))]
        public InventoryReason Reason { get; set; } = InventoryReason.Adjustment;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}