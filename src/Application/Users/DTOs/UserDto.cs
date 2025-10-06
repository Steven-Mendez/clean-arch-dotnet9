namespace Application.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string[] Roles,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);