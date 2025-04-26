using CommunityToolkit.Mvvm.ComponentModel;

namespace Bookstore.Mobile.Models
{
    public partial class CartItemDto : ObservableObject
    {
        public Guid BookId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public BookDto Book { get; set; } = null!;
        [ObservableProperty]
        private int _quantity;

        // Thuộc tính tính toán (không cần map từ API)
        public decimal TotalItemPrice => Book?.Price * Quantity ?? 0;
    }
}