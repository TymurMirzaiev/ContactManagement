using ContactManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using Xunit;

namespace ContactManagement.IntegrationTests.Infrastructure;

public class ContactManagementWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("YourStrong@Passw0rd123")
        .Build();

    public ContactManagementDbContext DbContext { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<ContactManagementDbContext>>();
            services.RemoveAll<ContactManagementDbContext>();

            // Add DbContext with test container connection string
            services.AddDbContext<ContactManagementDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });

            // Build service provider to get DbContext and apply migrations
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ContactManagementDbContext>();

            // Apply migrations
            dbContext.Database.Migrate();

            // Store DbContext for builder access
            DbContext = dbContext;
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }
}
