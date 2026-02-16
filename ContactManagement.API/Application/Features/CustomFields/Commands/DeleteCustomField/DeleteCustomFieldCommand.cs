using MediatR;

namespace ContactManagement.API.Application.Features.CustomFields.Commands.DeleteCustomField;

public record DeleteCustomFieldCommand(Guid Id) : IRequest<bool>;
