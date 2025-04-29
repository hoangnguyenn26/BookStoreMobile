namespace Bookstore.Mobile.Models
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Street { get; set; } = null!;
        public string Village { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;
        public bool IsDefault { get; set; }
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}