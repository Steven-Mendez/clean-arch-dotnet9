using Application.Users.DTOs;

namespace Application.Auth.DTOs;

public sealed record AuthResponseDto(string AccessToken, string RefreshToken, UserDto User);