﻿namespace Bookstore.Mobile.Models
{
    public class PromotionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public int CurrentUsage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string DiscountDisplay
        {
            get
            {
                if (DiscountPercentage.HasValue)
                    return $"{DiscountPercentage.Value}%";
                if (DiscountAmount.HasValue)
                    return $"{DiscountAmount.Value:C}";
                return "N/A";
            }
        }
    }
}