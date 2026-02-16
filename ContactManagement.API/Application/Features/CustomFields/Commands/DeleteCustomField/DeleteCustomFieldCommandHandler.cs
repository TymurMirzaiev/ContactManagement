using ContactManagement.API.Application.Common.Interfaces;
using MediatR;

namespace ContactManagement.API.Application.Features.CustomFields.Commands.DeleteCustomField;

public class DeleteCustomFieldCommandHandler : IRequestHandler<DeleteCustomFieldCommand, bool>
{
    private readonly IContactManagementDbContext _context;

    public DeleteCustomFieldCommandHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var customField = await _context.CustomFields.FindAsync(new object[] { request.Id }, cancellationToken);

        if (customField == null)
            return false;

        _context.CustomFields.Remove(customField);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
