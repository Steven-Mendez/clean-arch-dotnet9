using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, string RoleName) : ICommand<Unit>;