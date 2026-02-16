using ContractManagement.Data;
using ContractManagement.Entities;
using ContractManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Services;

public class CustomFieldService : ICustomFieldService
{
    private readonly ContactManagementDbContext _context;

    public CustomFieldService(ContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomFieldDto>> GetAllAsync()
    {
        var customFields = await _context.CustomFields.ToListAsync();
        return customFields.Select(cf => new CustomFieldDto
        {
            Id = cf.Id,
            Name = cf.Name,
            DataType = cf.DataType
        }).ToList();
    }

    public async Task<CustomFieldDto> CreateAsync(CreateCustomFieldDto createDto)
    {
        var customField = new CustomField
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            DataType = createDto.DataType
        };

        _context.CustomFields.Add(customField);
        await _context.SaveChangesAsync();

        return new CustomFieldDto
        {
            Id = customField.Id,
            Name = customField.Name,
            DataType = customField.DataType
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customField = await _context.CustomFields.FindAsync(id);
        if (customField == null) return false;

        _context.CustomFields.Remove(customField);
        await _context.SaveChangesAsync();
        return true;
    }
}
