using System.Text.Json;
using ContactManagement.API.Application.Common.Interfaces;
using ContactManagement.API.Domain.Entities;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.CreateContact;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, CreateContactResult>
{
    private readonly IContactManagementDbContext _context;

    public CreateContactCommandHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<CreateContactResult> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CustomFields = request.CustomFields != null
                ? JsonSerializer.Serialize(request.CustomFields)
                : "{}"
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateContactResult
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt,
            CustomFields = request.CustomFields
        };
    }
}
