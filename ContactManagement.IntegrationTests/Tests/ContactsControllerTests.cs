using System.Net;
using System.Net.Http.Json;
using ContactManagement.API.Application.Features.Contacts.Commands.CreateContact;
using ContactManagement.API.Application.Features.Contacts.Commands.UpdateContact;
using ContactManagement.IntegrationTests.Builders;
using ContactManagement.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactManagement.IntegrationTests.Tests;

public class ContactsControllerTests : IntegrationTestBase
{
    public ContactsControllerTests(ContactManagementWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateContact_WithValidData_ReturnsCreatedContact()
    {
        // Arrange
        var command = new CreateContactCommand
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "+1234567890",
            CustomFields = new Dictionary<string, object>
            {
                { "VIP", true },
                { "Age", 30 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateContactResult>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.Phone.Should().Be("+1234567890");
        result.CustomFields.Should().ContainKey("VIP");
        result.CustomFields!["VIP"].ToString().Should().Be("True");

        // Verify entity exists in database
        var contact = await DbContext.Contacts.FirstOrDefaultAsync(c => c.Id == result.Id);
        contact.Should().NotBeNull();
        contact!.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetContactById_WhenContactExists_ReturnsContact()
    {
        // Arrange
        var contact = await ContactBuilder.Create(DbContext)
            .WithName("Jane Smith")
            .WithEmail("jane.smith@example.com")
            .WithPhone("+9876543210")
            .WithCustomField("Department", "Engineering")
            .SaveAsync();

        // Act
        var response = await Client.GetAsync($"/api/contacts/{contact.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreateContactResult>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(contact.Id);
        result.Name.Should().Be("Jane Smith");
        result.Email.Should().Be("jane.smith@example.com");
        result.Phone.Should().Be("+9876543210");
        result.CustomFields.Should().ContainKey("Department");
    }

    [Fact]
    public async Task GetContactById_WhenContactDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/contacts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateContact_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var contact = await ContactBuilder.Create(DbContext)
            .WithName("Original Name")
            .WithEmail("original@example.com")
            .WithPhone("+1111111111")
            .SaveAsync();

        var updateCommand = new UpdateContactCommand
        {
            Id = contact.Id,
            Name = "Updated Name",
            Email = "updated@example.com",
            Phone = "+2222222222",
            CustomFields = new Dictionary<string, object>
            {
                { "Status", "Active" }
            }
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{contact.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify database updated
        // Detach the old entity to ensure we get fresh data from the database
        DbContext.Entry(contact).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedContact = await DbContext.Contacts.FirstOrDefaultAsync(c => c.Id == contact.Id);
        updatedContact.Should().NotBeNull();
        updatedContact!.Name.Should().Be("Updated Name");
        updatedContact.Email.Should().Be("updated@example.com");
        updatedContact.Phone.Should().Be("+2222222222");
        updatedContact.CustomFields.Should().Contain("Status");
    }

    [Fact]
    public async Task DeleteContact_WhenContactExists_ReturnsNoContent()
    {
        // Arrange
        var contact = await ContactBuilder.Create(DbContext)
            .WithName("To Be Deleted")
            .WithEmail("delete@example.com")
            .SaveAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contact.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted from database
        var deletedContact = await DbContext.Contacts.FirstOrDefaultAsync(c => c.Id == contact.Id);
        deletedContact.Should().BeNull();
    }
}
