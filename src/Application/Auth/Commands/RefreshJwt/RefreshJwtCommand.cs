using Application.Auth.DTOs;
using Cortex.Mediator.Commands;

namespace Application.Auth.Commands.RefreshJwt;

public sealed record RefreshJwtCommand(string RefreshToken) : ICommand<AuthResponseDto>;