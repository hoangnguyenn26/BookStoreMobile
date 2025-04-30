
using System.ComponentModel.DataAnnotations;
namespace Bookstore.Mobile.Models
{
    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}