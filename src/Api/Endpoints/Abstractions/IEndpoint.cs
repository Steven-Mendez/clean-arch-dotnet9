namespace Api.Endpoints.Abstractions;

public interface IEndpoint
{
    /// <summary>
    ///     Route group prefix used when registering this endpoint.
    /// </summary>
    string GroupPrefix => "/api/v1";

    /// <summary>
    ///     Default OpenAPI tag applied to the group containing this endpoint.
    /// </summary>
    string GroupTag => "API";

    /// <summary>
    ///     Configure the route(s) for this endpoint using the provided group builder.
    /// </summary>
    /// <param name="group">Feature route group this endpoint belongs to.</param>
    void MapEndpoint(RouteGroupBuilder group);
}