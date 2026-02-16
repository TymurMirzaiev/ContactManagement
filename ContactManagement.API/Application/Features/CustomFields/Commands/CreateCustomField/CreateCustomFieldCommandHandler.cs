using ContactManagement.API.Application.Common.Interfaces;
using ContactManagement.API.Domain.Entities;
using MediatR;

namespace ContactManagement.API.Application.Features.CustomFields.Commands.CreateCustomField;

public class CreateCustomFieldCommandHandler : IRequestHandler<CreateCustomFieldCommand, CreateCustomFieldResult>
{
    private readonly IContactManagementDbContext _context;

    public CreateCustomFieldCommandHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<CreateCustomFieldResult> Handle(CreateCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var customField = new CustomField
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DataType = request.DataType
        };

        _context.CustomFields.Add(customField);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCustomFieldResult
        {
            Id = customField.Id,
            Name = customField.Name,
            DataType = customField.DataType
        };
    }
}
