
using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IDashboardApi
    {
        [Get("/v1/home/dashboard")] // Đường dẫn API Dashboard
        Task<ApiResponse<HomeDashboardDto>> GetHomeDashboard();
    }
}