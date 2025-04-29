
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Mobile.Models
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}