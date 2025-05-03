using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler (Admin/Staff)
    public interface IInventoryApi
    {
        [Post("/admin/inventory/adjust")]
        Task<ApiResponse<int>> AdjustStock([Body] AdjustInventoryRequestDto adjustDto);
    }
}