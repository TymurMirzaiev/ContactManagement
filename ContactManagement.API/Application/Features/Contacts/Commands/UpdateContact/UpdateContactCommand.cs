using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.UpdateContact;

public record UpdateContactCommand : IRequest<bool>
{
    public Guid Id { get; init; }

    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(50)]
    public string Phone { get; init; } = string.Empty;

    public Dictionary<string, object>? CustomFields { get; init; }
}
