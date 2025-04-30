using Bookstore.Mobile.Models;
using Refit;
using System.Threading.Tasks;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IAdminDashboardApi
    {
        [Get("/admin/dashboard/summary")]
        Task<ApiResponse<AdminDashboardSummaryDto>> GetSummary([Query] int lowStockThreshold = 5);
    }
}