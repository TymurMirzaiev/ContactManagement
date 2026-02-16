using ContactManagement.API.Application.Common.Models;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Queries.GetAllContacts;

public record GetAllContactsQuery(
    int Page,
    int PageSize,
    string? SortBy,
    string? FilterEmail
) : IRequest<PagedResult<ContactDto>>;
