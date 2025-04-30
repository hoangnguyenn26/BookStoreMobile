
using System.ComponentModel.DataAnnotations;
namespace Bookstore.Mobile.Models
{
    public class UpdateUserRolesDto
    {
        [Required]
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}