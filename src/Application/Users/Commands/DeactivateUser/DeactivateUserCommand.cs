using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : ICommand<Unit>;