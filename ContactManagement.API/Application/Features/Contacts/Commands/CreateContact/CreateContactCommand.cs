using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.CreateContact;

public record CreateContactCommand : IRequest<CreateContactResult>
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(50)]
    public string Phone { get; init; } = string.Empty;

    public Dictionary<string, object>? CustomFields { get; init; }
}

public record CreateContactResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Dictionary<string, object>? CustomFields { get; init; }
}
