using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler (Admin)
    public interface IAdminReportApi
    {
        [Get("/admin/reports/revenue")]
        Task<ApiResponse<RevenueReportDto>> GetRevenueReport([Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/admin/reports/bestsellers")]
        Task<ApiResponse<IEnumerable<BestsellerDto>>> GetBestsellersReport([Query] DateTime startDate, [Query] DateTime endDate, [Query] int top = 5);

        [Get("/admin/reports/stock")]
        Task<ApiResponse<IEnumerable<LowStockBookDto>>> GetLowStockReport([Query] int threshold = 5);
    }
}