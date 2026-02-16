using System.ComponentModel.DataAnnotations;

namespace ContractManagement.Models;

public class CustomFieldDto
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(string|int|bool)$", ErrorMessage = "DataType must be 'string', 'int', or 'bool'")]
    public string DataType { get; set; } = "string";
}

public class CreateCustomFieldDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(string|int|bool)$", ErrorMessage = "DataType must be 'string', 'int', or 'bool'")]
    public string DataType { get; set; } = "string";
}

public class AssignCustomFieldValueDto
{
    [Required]
    public object Value { get; set; } = null!;
}
