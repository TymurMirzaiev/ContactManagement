using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ContactManagement.API.Application.Features.CustomFields.Commands.CreateCustomField;

public record CreateCustomFieldCommand : IRequest<CreateCustomFieldResult>
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, RegularExpression("^(string|int|bool)$", ErrorMessage = "DataType must be 'string', 'int', or 'bool'")]
    public string DataType { get; init; } = "string";
}

public record CreateCustomFieldResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DataType { get; init; } = "string";
}
