using Microsoft.AspNetCore.Identity;

namespace GymManagementSystem.DAL.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = default!;
    }
}
