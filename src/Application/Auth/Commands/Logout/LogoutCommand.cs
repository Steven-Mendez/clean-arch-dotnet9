using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid UserId, string RefreshToken) : ICommand<Unit>;