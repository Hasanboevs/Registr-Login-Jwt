using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtApi.Data
{
    public class AppDbContex(DbContextOptions options) : IdentityDbContext<ApplicationUser>(options)
    {
    }
}
