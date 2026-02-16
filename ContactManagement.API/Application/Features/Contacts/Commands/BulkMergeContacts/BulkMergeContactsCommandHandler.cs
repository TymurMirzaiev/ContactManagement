using System.Text.Json;
using ContactManagement.API.Application.Common.Interfaces;
using ContactManagement.API.Domain.Entities;
using ContactManagement.API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.API.Application.Features.Contacts.Commands.BulkMergeContacts;

public class BulkMergeContactsCommandHandler : IRequestHandler<BulkMergeContactsCommand>
{
    private readonly ContactManagementDbContext _context;

    public BulkMergeContactsCommandHandler(ContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task Handle(BulkMergeContactsCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var emails = request.Contacts.Select(c => c.Email).ToHashSet();
            var existingContacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email))
                .ToListAsync(cancellationToken);

            var existingContactsDict = existingContacts.ToDictionary(c => c.Email);

            // Update existing and track new ones
            foreach (var contactDto in request.Contacts)
            {
                if (existingContactsDict.TryGetValue(contactDto.Email, out var existing))
                {
                    // Update existing
                    existing.Name = contactDto.Name;
                    existing.Phone = contactDto.Phone;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.CustomFields = contactDto.CustomFields != null
                        ? JsonSerializer.Serialize(contactDto.CustomFields)
                        : "{}";
                    existingContactsDict.Remove(contactDto.Email);
                }
                else
                {
                    // Create new
                    var newContact = new Contact
                    {
                        Id = Guid.NewGuid(),
                        Name = contactDto.Name,
                        Email = contactDto.Email,
                        Phone = contactDto.Phone,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CustomFields = contactDto.CustomFields != null
                            ? JsonSerializer.Serialize(contactDto.CustomFields)
                            : "{}"
                    };
                    _context.Contacts.Add(newContact);
                }
            }

            // Remove contacts not in the bulk merge list
            var contactsToRemove = await _context.Contacts
                .Where(c => !emails.Contains(c.Email))
                .ToListAsync(cancellationToken);

            _context.Contacts.RemoveRange(contactsToRemove);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
