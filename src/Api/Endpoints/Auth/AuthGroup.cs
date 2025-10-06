namespace Api.Endpoints.Auth;

/// <summary>
/// Shared configuration values for authentication endpoints.
/// </summary>
public static class AuthGroup
{
    public const string Prefix = "/api/v1/auth";
    public const string Tag = "Auth";

    /// <summary>
    /// Builds an absolute route for the auth group from the provided relative <paramref name="route"/>.
    /// </summary>
    /// <param name="route">Relative endpoint route (e.g. "/login").</param>
    public static string Absolute(string route) => string.Concat(Prefix, route);

    /// <summary>
    /// Applies shared auth configuration (tagging, authorization) to the provided <paramref name="group"/>.
    /// </summary>
    /// <param name="group">Route group for auth endpoints.</param>
    public static RouteGroupBuilder Configure(RouteGroupBuilder group)
    {
        return group.WithTags(Tag);
    }
}
