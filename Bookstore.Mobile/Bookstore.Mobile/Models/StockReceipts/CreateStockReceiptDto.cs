
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class CreateStockReceiptDto
    {
        public Guid? SupplierId { get; set; }

        [Required]
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateStockReceiptDetailDto> Details { get; set; } = new List<CreateStockReceiptDetailDto>();
    }
}