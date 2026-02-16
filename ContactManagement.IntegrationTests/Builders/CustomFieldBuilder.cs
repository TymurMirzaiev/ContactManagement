using ContactManagement.API.Domain.Entities;
using ContactManagement.API.Infrastructure.Data;

namespace ContactManagement.IntegrationTests.Builders;

public class CustomFieldBuilder
{
    private readonly ContactManagementDbContext _context;
    private Guid _id = Guid.NewGuid();
    private string _name = $"CustomField-{Guid.NewGuid()}"; // Unique by default
    private string _dataType = "string";

    private static readonly HashSet<string> ValidDataTypes = new() { "string", "int", "bool" };

    private CustomFieldBuilder(ContactManagementDbContext context)
    {
        _context = context;
    }

    public static CustomFieldBuilder Create(ContactManagementDbContext context)
    {
        return new CustomFieldBuilder(context);
    }

    public CustomFieldBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CustomFieldBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CustomFieldBuilder WithDataType(string dataType)
    {
        if (!ValidDataTypes.Contains(dataType))
        {
            throw new ArgumentException(
                $"Invalid data type '{dataType}'. Must be one of: {string.Join(", ", ValidDataTypes)}");
        }

        _dataType = dataType;
        return this;
    }

    public CustomFieldBuilder AsString()
    {
        _dataType = "string";
        return this;
    }

    public CustomFieldBuilder AsInt()
    {
        _dataType = "int";
        return this;
    }

    public CustomFieldBuilder AsBool()
    {
        _dataType = "bool";
        return this;
    }

    public CustomField Build()
    {
        return new CustomField
        {
            Id = _id,
            Name = _name,
            DataType = _dataType
        };
    }

    public async Task<CustomField> SaveAsync()
    {
        var customField = Build();
        _context.CustomFields.Add(customField);
        await _context.SaveChangesAsync();
        return customField;
    }

    public async Task<List<CustomField>> SaveManyAsync(int count)
    {
        var customFields = new List<CustomField>();

        for (int i = 0; i < count; i++)
        {
            // Reset name to ensure uniqueness for each field
            var customField = WithName($"CustomField-{Guid.NewGuid()}").Build();
            customFields.Add(customField);
            _context.CustomFields.Add(customField);
        }

        await _context.SaveChangesAsync();
        return customFields;
    }
}
