using Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed;

/// <summary>
///     Hosted service that ensures baseline roles exist on application startup.
/// </summary>
public sealed class RoleSeederHostedService : IHostedService
{
    private static readonly string[] DefaultRoles = ["User", "Admin"];
    private readonly ILogger<RoleSeederHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RoleSeederHostedService(IServiceScopeFactory scopeFactory, ILogger<RoleSeederHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await roleRepository.EnsureSeedAsync(DefaultRoles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Role seeding completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}