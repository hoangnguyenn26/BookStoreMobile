using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    // Cần AuthHeaderHandler (Admin/Staff)
    public interface IStockReceiptApi
    {
        [Get("/admin/stock-receipts")]
        Task<ApiResponse<IEnumerable<StockReceiptDto>>> GetAllReceipts([Query] int page = 1, [Query] int pageSize = 10);

        [Get("/admin/stock-receipts/{id}")]
        Task<ApiResponse<StockReceiptDto>> GetReceiptById(Guid id);

        [Post("/admin/stock-receipts")]
        Task<ApiResponse<StockReceiptDto>> CreateReceipt([Body] CreateStockReceiptDto receipt);
    }
}