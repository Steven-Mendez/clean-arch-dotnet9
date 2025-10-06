namespace Api.Endpoints.Users;

public static class UsersGroup
{
    public const string Prefix = "/api/v1/users";
    public const string Tag = "Users";

    public static RouteGroupBuilder Configure(RouteGroupBuilder group)
    {
        return group.WithTags(Tag).RequireAuthorization();
    }
}