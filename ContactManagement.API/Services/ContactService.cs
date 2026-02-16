using System.Text.Json;
using ContractManagement.Data;
using ContractManagement.Entities;
using ContractManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Services;

public class ContactService : IContactService
{
    private readonly ContactManagementDbContext _context;

    public ContactService(ContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ContactDto>> GetAllAsync(int page, int pageSize, string? sortBy, string? filterEmail)
    {
        var query = _context.Contacts.AsQueryable();

        // Apply email filter if provided
        if (!string.IsNullOrWhiteSpace(filterEmail))
        {
            query = query.Where(c => c.Email.Contains(filterEmail));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => query.OrderBy(c => c.Name),
            "email" => query.OrderBy(c => c.Email),
            "createdat" => query.OrderBy(c => c.CreatedAt),
            "createdat_desc" => query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt) // Default sort
        };

        var totalCount = await query.CountAsync();

        var contacts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ContactDto>
        {
            Items = contacts.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        return contact != null ? MapToDto(contact) : null;
    }

    public async Task<ContactDto> CreateAsync(CreateContactDto createDto)
    {
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Email = createDto.Email,
            Phone = createDto.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CustomFields = createDto.CustomFields != null
                ? JsonSerializer.Serialize(createDto.CustomFields)
                : "{}"
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        return MapToDto(contact);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateContactDto updateDto)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null) return false;

        contact.Name = updateDto.Name;
        contact.Email = updateDto.Email;
        contact.Phone = updateDto.Phone;
        contact.UpdatedAt = DateTime.UtcNow;
        contact.CustomFields = updateDto.CustomFields != null
            ? JsonSerializer.Serialize(updateDto.CustomFields)
            : "{}";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null) return false;

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task BulkMergeAsync(List<BulkMergeContactDto> contacts)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var emails = contacts.Select(c => c.Email).ToHashSet();
            var existingContacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email))
                .ToListAsync();

            var existingContactsDict = existingContacts.ToDictionary(c => c.Email);

            // Update existing and track new ones
            foreach (var contactDto in contacts)
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
                .ToListAsync();

            _context.Contacts.RemoveRange(contactsToRemove);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> AssignCustomFieldValueAsync(Guid contactId, Guid customFieldId, object value)
    {
        var contact = await _context.Contacts.FindAsync(contactId);
        if (contact == null) return false;

        var customField = await _context.CustomFields.FindAsync(customFieldId);
        if (customField == null) return false;

        var customFieldsDict = string.IsNullOrWhiteSpace(contact.CustomFields)
            ? new Dictionary<string, object>()
            : JsonSerializer.Deserialize<Dictionary<string, object>>(contact.CustomFields) ?? new Dictionary<string, object>();

        customFieldsDict[customField.Name] = value;

        contact.CustomFields = JsonSerializer.Serialize(customFieldsDict);
        contact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static ContactDto MapToDto(Contact contact)
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
    }
}
