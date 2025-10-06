using System.Reflection;

namespace Api.Endpoints.Abstractions;

public static class EndpointDiscoveryExtensions
{
    public static IEndpointRouteBuilder MapDiscoveredEndpoints(
        this IEndpointRouteBuilder app,
        params Assembly[] assemblies)
    {
        var targets = assemblies is { Length: > 0 }
            ? assemblies
            : new[] { Assembly.GetExecutingAssembly() };

        var endpointTypes = targets
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t))
            .ToArray();

        var instances = endpointTypes
            .Select(t => (IEndpoint)Activator.CreateInstance(t.AsType())!)
            .GroupBy(e => (e.GroupPrefix, e.GroupTag));

        foreach (var group in instances)
        {
            var (prefix, tag) = group.Key;
            var routeGroup = app.MapGroup(prefix).WithTags(tag);

            foreach (var endpoint in group) endpoint.MapEndpoint(routeGroup);
        }

        return app;
    }
}