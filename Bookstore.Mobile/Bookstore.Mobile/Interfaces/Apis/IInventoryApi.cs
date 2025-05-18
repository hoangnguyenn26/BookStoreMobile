using Bookstore.Mobile.Enums;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Models.Inventory;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler (Admin/Staff)
    public interface IInventoryApi
    {
        [Post("/admin/inventory/adjust")]
        Task<ApiResponse<int>> AdjustStock([Body] AdjustInventoryRequestDto adjustDto);
        [Get("/admin/inventory/history")]
        Task<ApiResponse<PagedInventoryLogResult>> GetInventoryHistory(
            [Query] Guid? bookId = null,
            [Query] InventoryReason? reason = null,
            [Query] DateTime? startDate = null,
            [Query] DateTime? endDate = null,
            [Query] Guid? performedByUserId = null,
            [Query] Guid? orderId = null,
            [Query] Guid? stockReceiptId = null,
            [Query] int page = 1,
            [Query] int pageSize = 20);
    }
}