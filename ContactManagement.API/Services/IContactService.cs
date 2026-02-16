using ContractManagement.Models;

namespace ContractManagement.Services;

public interface IContactService
{
    Task<PagedResult<ContactDto>> GetAllAsync(int page, int pageSize, string? sortBy, string? filterEmail);
    Task<ContactDto?> GetByIdAsync(Guid id);
    Task<ContactDto> CreateAsync(CreateContactDto createDto);
    Task<bool> UpdateAsync(Guid id, UpdateContactDto updateDto);
    Task<bool> DeleteAsync(Guid id);
    Task BulkMergeAsync(List<BulkMergeContactDto> contacts);
    Task<bool> AssignCustomFieldValueAsync(Guid contactId, Guid customFieldId, object value);
}
