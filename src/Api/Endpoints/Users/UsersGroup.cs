namespace Api.Endpoints.Users;

/// <summary>
/// Shared configuration values for user-management endpoints.
/// </summary>
public static class UsersGroup
{
    public const string Prefix = "/api/v1/users";
    public const string Tag = "Users";

    /// <summary>
    /// Applies shared user endpoint configuration (tagging, authorization) to the provided <paramref name="group"/>.
    /// </summary>
    /// <param name="group">Route group for user-management endpoints.</param>
    public static RouteGroupBuilder Configure(RouteGroupBuilder group)
    {
        return group.WithTags(Tag).RequireAuthorization();
    }
}
