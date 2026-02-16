using System.Text.Json;
using ContactManagement.API.Application.Common.Interfaces;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Queries.GetContactById;

public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDto?>
{
    private readonly IContactManagementDbContext _context;

    public GetContactByIdQueryHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ContactDto?> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (contact == null)
            return null;

        Dictionary<string, object>? customFields = null;
        if (!string.IsNullOrWhiteSpace(contact.CustomFields) && contact.CustomFields != "{}")
        {
            customFields = JsonSerializer.Deserialize<Dictionary<string, object>>(contact.CustomFields);
        }

        return new ContactDto
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt,
            CustomFields = customFields
        };
    }
}
