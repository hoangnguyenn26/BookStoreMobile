
namespace Bookstore.Mobile.Models
{
    public class BookSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? AuthorName { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal Price { get; set; }
    }
}