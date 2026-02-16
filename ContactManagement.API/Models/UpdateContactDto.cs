using System.ComponentModel.DataAnnotations;

namespace ContractManagement.Models;

public class UpdateContactDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    public Dictionary<string, object>? CustomFields { get; set; }
}
