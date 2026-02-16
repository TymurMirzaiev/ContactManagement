using ContactManagement.API.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContactManagement.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<ContactManagementWebApplicationFactory>, IAsyncLifetime
{
    protected readonly ContactManagementWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly ContactManagementDbContext DbContext;

    protected IntegrationTestBase(ContactManagementWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();

        // Get a new scope for DbContext to ensure fresh instance per test
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ContactManagementDbContext>();
    }

    public Task InitializeAsync()
    {
        // No initialization needed per test
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Clean up database after each test for test isolation
        DbContext.Contacts.RemoveRange(DbContext.Contacts);
        DbContext.CustomFields.RemoveRange(DbContext.CustomFields);
        await DbContext.SaveChangesAsync();
    }
}
