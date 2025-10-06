using System.Security.Claims;

namespace Api.Extensions;

/// <summary>
///     Helper extensions for extracting common values from <see cref="ClaimsPrincipal" />.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    ///     Attempts to parse the caller's user id from JWT or standard claim types.
    /// </summary>
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        return Guid.TryParse(value, out userId);
    }
}