
namespace Bookstore.Mobile.Models
{
    public class OrderShippingAddressDto
    {
        public Guid Id { get; set; }
        public string Street { get; set; } = null!;
        public string? Village { get; set; }
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}