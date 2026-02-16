using ContactManagement.API.Application.Common.Interfaces;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.DeleteContact;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, bool>
{
    private readonly IContactManagementDbContext _context;

    public DeleteContactCommandHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (contact == null)
            return false;

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
