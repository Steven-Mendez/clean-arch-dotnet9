using Application.Users.DTOs;
using Cortex.Mediator.Queries;

namespace Application.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<UserDto>;