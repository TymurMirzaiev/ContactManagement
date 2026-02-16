using ContactManagement.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.API.Application.Common.Interfaces;

public interface IContactManagementDbContext
{
    DbSet<Contact> Contacts { get; }
    DbSet<CustomField> CustomFields { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
