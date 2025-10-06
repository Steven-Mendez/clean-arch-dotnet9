using Application.Users.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Common.Mapping;

/// <summary>
///     Centralizes Mapster configuration for the application layer.
/// </summary>
public static class MapsterConfig
{
    /// <summary>
    ///     Registers mapping rules on the provided Mapster configuration object.
    /// </summary>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Roles, _ => Array.Empty<string>());
    }
}