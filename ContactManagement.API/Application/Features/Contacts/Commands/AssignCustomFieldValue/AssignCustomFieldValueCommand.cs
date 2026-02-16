using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.AssignCustomFieldValue;

public record AssignCustomFieldValueCommand(
    Guid ContactId,
    Guid CustomFieldId,
    object Value
) : IRequest<bool>;
