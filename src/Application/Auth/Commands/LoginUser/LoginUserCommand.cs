using Application.Auth.DTOs;
using Cortex.Mediator.Commands;

namespace Application.Auth.Commands.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AuthResponseDto>;