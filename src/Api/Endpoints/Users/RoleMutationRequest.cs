namespace Api.Endpoints.Users;

/// <summary>
/// Request payload representing a role assignment or removal operation.
/// </summary>
/// <param name="RoleName">Name of the role being granted or revoked.</param>
public sealed record RoleMutationRequest(string RoleName);
