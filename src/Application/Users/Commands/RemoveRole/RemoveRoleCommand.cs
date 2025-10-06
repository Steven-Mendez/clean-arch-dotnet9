using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.RemoveRole;

public sealed record RemoveRoleCommand(Guid UserId, string RoleName) : ICommand<Unit>;