
namespace Bookstore.Mobile.Models
{
    public class AdminDashboardSummaryDto
    {
        public int NewOrdersToday { get; set; }
        public decimal TotalRevenueToday { get; set; }
        public int NewUsersToday { get; set; }
        public int LowStockItemsCount { get; set; }
    }
}