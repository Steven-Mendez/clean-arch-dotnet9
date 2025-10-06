using Application.Users.DTOs;
using Cortex.Mediator.Commands;

namespace Application.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password, string DisplayName) : ICommand<UserDto>;