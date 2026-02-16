using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Queries.GetContactById;

public record GetContactByIdQuery(Guid Id) : IRequest<ContactDto?>;
