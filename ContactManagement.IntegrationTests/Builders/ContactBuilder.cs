using System.Text.Json;
using ContactManagement.API.Domain.Entities;
using ContactManagement.API.Infrastructure.Data;

namespace ContactManagement.IntegrationTests.Builders;

public class ContactBuilder
{
    private readonly ContactManagementDbContext _context;
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Contact";
    private string _email = $"test-{Guid.NewGuid()}@example.com"; // Unique by default
    private string _phone = "+1234567890";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private Dictionary<string, object> _customFieldsDict = new();

    private ContactBuilder(ContactManagementDbContext context)
    {
        _context = context;
    }

    public static ContactBuilder Create(ContactManagementDbContext context)
    {
        return new ContactBuilder(context);
    }

    public ContactBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ContactBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ContactBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ContactBuilder WithPhone(string phone)
    {
        _phone = phone;
        return this;
    }

    public ContactBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ContactBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public ContactBuilder WithCustomField(string name, object value)
    {
        _customFieldsDict[name] = value;
        return this;
    }

    public ContactBuilder WithCustomFields(Dictionary<string, object> customFields)
    {
        _customFieldsDict = customFields;
        return this;
    }

    public Contact Build()
    {
        var customFieldsJson = _customFieldsDict.Any()
            ? JsonSerializer.Serialize(_customFieldsDict)
            : "{}";

        return new Contact
        {
            Id = _id,
            Name = _name,
            Email = _email,
            Phone = _phone,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            CustomFields = customFieldsJson
        };
    }

    public async Task<Contact> SaveAsync()
    {
        var contact = Build();
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<List<Contact>> SaveManyAsync(int count)
    {
        var contacts = new List<Contact>();

        for (int i = 0; i < count; i++)
        {
            // Reset email to ensure uniqueness for each contact
            var contact = WithEmail($"test-{Guid.NewGuid()}@example.com").Build();
            contacts.Add(contact);
            _context.Contacts.Add(contact);
        }

        await _context.SaveChangesAsync();
        return contacts;
    }
}
