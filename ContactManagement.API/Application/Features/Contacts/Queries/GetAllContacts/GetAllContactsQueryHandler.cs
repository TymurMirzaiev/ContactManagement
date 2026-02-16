using System.Text.Json;
using ContactManagement.API.Application.Common.Interfaces;
using ContactManagement.API.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.API.Application.Features.Contacts.Queries.GetAllContacts;

public class GetAllContactsQueryHandler : IRequestHandler<GetAllContactsQuery, PagedResult<ContactDto>>
{
    private readonly IContactManagementDbContext _context;

    public GetAllContactsQueryHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ContactDto>> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Contacts.AsQueryable();

        // Apply email filter if provided
        if (!string.IsNullOrWhiteSpace(request.FilterEmail))
        {
            query = query.Where(c => c.Email.Contains(request.FilterEmail));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => query.OrderBy(c => c.Name),
            "email" => query.OrderBy(c => c.Email),
            "createdat" => query.OrderBy(c => c.CreatedAt),
            "createdat_desc" => query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt) // Default sort
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var contacts = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var contactDtos = contacts.Select(contact =>
        {
            Dictionary<string, object>? customFields = null;
            if (!string.IsNullOrWhiteSpace(contact.CustomFields) && contact.CustomFields != "{}")
            {
                customFields = JsonSerializer.Deserialize<Dictionary<string, object>>(contact.CustomFields);
            }

            return new ContactDto
            {
                Id = contact.Id,
                Name = contact.Name,
                Email = contact.Email,
                Phone = contact.Phone,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt,
                CustomFields = customFields
            };
        }).ToList();

        return new PagedResult<ContactDto>
        {
            Items = contactDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
