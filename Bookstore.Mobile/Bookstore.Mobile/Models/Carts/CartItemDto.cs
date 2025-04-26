namespace Bookstore.Mobile.Models
{
    public class CartItemDto
    {
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public BookDto Book { get; set; } = null!;

        // Thuộc tính tính toán (không cần map từ API)
        public decimal TotalItemPrice => Book?.Price * Quantity ?? 0;
    }
}