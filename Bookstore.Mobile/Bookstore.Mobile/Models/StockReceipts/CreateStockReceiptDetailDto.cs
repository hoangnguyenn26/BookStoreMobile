
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class CreateStockReceiptDetailDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Range(1, int.MaxValue)]
        public int QuantityReceived { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        public decimal? PurchasePrice { get; set; } // Cho phép null
    }
}