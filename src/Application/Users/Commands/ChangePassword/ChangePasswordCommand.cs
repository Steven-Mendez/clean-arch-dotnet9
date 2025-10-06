using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid TargetUserId,
    Guid RequestingUserId,
    string? CurrentPassword,
    string NewPassword,
    bool IsAdmin) : ICommand<Unit>;