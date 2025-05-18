namespace Bookstore.Mobile.Models.Inventory
{
    public class PagedInventoryLogResult
    {
        public IEnumerable<InventoryLogDto> Items { get; set; } = new List<InventoryLogDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
