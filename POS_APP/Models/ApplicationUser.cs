using Microsoft.AspNetCore.Identity;

namespace POS_APP.Models
{
    public class ApplicationUser : IdentityUser
    {
        // เพิ่ม custom property ได้ เช่น
        public string? FullName { get; set; }
    }
}
