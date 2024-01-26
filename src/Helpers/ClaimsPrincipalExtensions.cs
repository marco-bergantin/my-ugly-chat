using System.Security.Claims;

namespace Auth0Mvc.Helpers;

public static class ClaimsPrincipalExtensions
{
    public const string UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

    public static string? GetUserId(this ClaimsPrincipal user) =>
        user?.Claims?.SingleOrDefault(c => c.Type.Equals(UserIdClaimType, StringComparison.InvariantCultureIgnoreCase))?.Value;
}
