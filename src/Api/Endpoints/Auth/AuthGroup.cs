namespace Api.Endpoints.Auth;

public static class AuthGroup
{
    public const string Prefix = "/api/v1/auth";
    public const string Tag = "Auth";

    public static RouteGroupBuilder Configure(RouteGroupBuilder group)
    {
        return group.WithTags(Tag);
    }
}