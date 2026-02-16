using System.Text.Json;
using ContactManagement.API.Application.Common.Interfaces;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.AssignCustomFieldValue;

public class AssignCustomFieldValueCommandHandler : IRequestHandler<AssignCustomFieldValueCommand, bool>
{
    private readonly IContactManagementDbContext _context;

    public AssignCustomFieldValueCommandHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AssignCustomFieldValueCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contacts.FindAsync(new object[] { request.ContactId }, cancellationToken);
        if (contact == null)
            return false;

        var customField = await _context.CustomFields.FindAsync(new object[] { request.CustomFieldId }, cancellationToken);
        if (customField == null)
            return false;

        var customFieldsDict = string.IsNullOrWhiteSpace(contact.CustomFields)
            ? new Dictionary<string, object>()
            : JsonSerializer.Deserialize<Dictionary<string, object>>(contact.CustomFields) ?? new Dictionary<string, object>();

        customFieldsDict[customField.Name] = request.Value;

        contact.CustomFields = JsonSerializer.Serialize(customFieldsDict);
        contact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
