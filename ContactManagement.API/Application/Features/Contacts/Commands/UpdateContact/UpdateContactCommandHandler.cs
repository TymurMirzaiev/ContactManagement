using System.Text.Json;
using ContactManagement.API.Application.Common.Interfaces;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.UpdateContact;

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, bool>
{
    private readonly IContactManagementDbContext _context;

    public UpdateContactCommandHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (contact == null)
            return false;

        contact.Name = request.Name;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.UpdatedAt = DateTime.UtcNow;
        contact.CustomFields = request.CustomFields != null
            ? JsonSerializer.Serialize(request.CustomFields)
            : "{}";

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
