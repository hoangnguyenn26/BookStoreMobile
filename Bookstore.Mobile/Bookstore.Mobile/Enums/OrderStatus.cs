
namespace Bookstore.Mobile.Enums
{
    public enum OrderStatus : byte
    {
        Pending = 0,
        Confirmed = 1,
        Shipping = 2,
        Completed = 3,
        Cancelled = 4
    }
}