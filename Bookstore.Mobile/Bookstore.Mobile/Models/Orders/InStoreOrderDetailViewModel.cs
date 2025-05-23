using CommunityToolkit.Mvvm.ComponentModel;

namespace Bookstore.Mobile.Models.Orders
{
    public partial class InStoreOrderDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private BookDto _selectedBook; // Thông tin sách

        [ObservableProperty]
        private int _quantity;

        public Guid BookId => SelectedBook.Id;
        public string BookTitle => SelectedBook.Title;
        public decimal UnitPrice => SelectedBook.Price; // Lấy giá hiện tại của sách
        public decimal TotalItemPrice => UnitPrice * Quantity;

        public InStoreOrderDetailViewModel(BookDto book, int quantity)
        {
            SelectedBook = book;
            Quantity = quantity;
        }
    }
}