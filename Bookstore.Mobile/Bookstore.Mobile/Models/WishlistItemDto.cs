namespace Bookstore.Mobile.Models
{
    public class WishlistItemDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public BookDto Book { get; set; } = null!;
    }
}