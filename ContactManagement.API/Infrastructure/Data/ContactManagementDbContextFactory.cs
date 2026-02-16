using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContactManagement.API.Infrastructure.Data;

public class ContactManagementDbContextFactory : IDesignTimeDbContextFactory<ContactManagementDbContext>
{
    public ContactManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContactManagementDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=ContactManagement;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;");

        return new ContactManagementDbContext(optionsBuilder.Options);
    }
}
