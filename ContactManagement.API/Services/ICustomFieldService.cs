using ContractManagement.Models;

namespace ContractManagement.Services;

public interface ICustomFieldService
{
    Task<List<CustomFieldDto>> GetAllAsync();
    Task<CustomFieldDto> CreateAsync(CreateCustomFieldDto createDto);
    Task<bool> DeleteAsync(Guid id);
}
