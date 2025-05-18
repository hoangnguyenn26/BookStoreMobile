using Bookstore.Mobile.Enums;

namespace Bookstore.Mobile.Models
{
    public class InventoryLogDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public int ChangeQuantity { get; set; }
        public InventoryReason Reason { get; set; }
        public DateTime TimestampUtc { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? StockReceiptId { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Notes { get; set; }
    }
}