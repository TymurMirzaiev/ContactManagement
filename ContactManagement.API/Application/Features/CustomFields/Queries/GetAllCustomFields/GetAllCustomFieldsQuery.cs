using MediatR;

namespace ContactManagement.API.Application.Features.CustomFields.Queries.GetAllCustomFields;

public record GetAllCustomFieldsQuery : IRequest<List<CustomFieldDto>>;
