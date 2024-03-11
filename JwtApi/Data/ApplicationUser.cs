using Microsoft.AspNetCore.Identity;

namespace JwtApi.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
