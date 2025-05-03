using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface ISupplierApi
    {
        // GET: api/admin/suppliers
        [Get("/admin/suppliers")]
        Task<ApiResponse<IEnumerable<SupplierDto>>> GetAllSuppliers();

        // GET: api/admin/suppliers/{id}
        [Get("/admin/suppliers/{id}")]
        Task<ApiResponse<SupplierDto>> GetSupplierById(Guid id);

        // POST: api/admin/suppliers
        [Post("/admin/suppliers")]
        Task<ApiResponse<SupplierDto>> CreateSupplier([Body] CreateSupplierDto createDto);

        // PUT: api/admin/suppliers/{id}
        [Put("/admin/suppliers/{id}")]
        Task<ApiResponse<object>> UpdateSupplier(Guid id, [Body] UpdateSupplierDto updateDto);

        // DELETE: api/admin/suppliers/{id}
        [Delete("/admin/suppliers/{id}")]
        Task<ApiResponse<object>> DeleteSupplier(Guid id);
    }
}