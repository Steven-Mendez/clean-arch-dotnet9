namespace Api.Endpoints.Abstractions;

/// <summary>
/// Describes the HTTP surface of an endpoint so its verb and route are visible at a glance.
/// </summary>
/// <param name="HttpVerb">HTTP verb used to invoke the endpoint (e.g. GET, POST).</param>
/// <param name="Route">Route template relative to the owning route group.</param>
/// <param name="Name">Human-friendly endpoint name used in routing metadata.</param>
/// <param name="Summary">Short description used for documentation.</param>
/// <param name="Description">Long-form description used for documentation.</param>
public readonly record struct EndpointDescriptor(
    string HttpVerb,
    string Route,
    string Name,
    string Summary,
    string? Description = null)
{
    /// <summary>
    /// Full absolute route including the group's prefix.
    /// </summary>
    /// <param name="groupPrefix">Route group prefix.</param>
    public string BuildAbsoluteRoute(string groupPrefix) => string.Concat(groupPrefix, Route);

    public override string ToString() => $"{HttpVerb} {Route}";
}
