using Application.Users.DTOs;
using Cortex.Mediator.Queries;

namespace Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;