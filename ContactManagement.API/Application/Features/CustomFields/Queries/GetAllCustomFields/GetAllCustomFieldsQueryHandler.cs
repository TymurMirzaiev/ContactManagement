using ContactManagement.API.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.API.Application.Features.CustomFields.Queries.GetAllCustomFields;

public class GetAllCustomFieldsQueryHandler : IRequestHandler<GetAllCustomFieldsQuery, List<CustomFieldDto>>
{
    private readonly IContactManagementDbContext _context;

    public GetAllCustomFieldsQueryHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomFieldDto>> Handle(GetAllCustomFieldsQuery request, CancellationToken cancellationToken)
    {
        var customFields = await _context.CustomFields.ToListAsync(cancellationToken);

        return customFields.Select(cf => new CustomFieldDto
        {
            Id = cf.Id,
            Name = cf.Name,
            DataType = cf.DataType
        }).ToList();
    }
}
