using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.DeleteContact;

public record DeleteContactCommand(Guid Id) : IRequest<bool>;
