using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LapNotes.Api;

// Fail closed when the issuer supplies no organization or multiple conflicting organizations.
public sealed class OrganizationRequirement : IAuthorizationRequirement;

public sealed class OrganizationAuthorizationHandler
    : AuthorizationHandler<OrganizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OrganizationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && OrganizationScope.TryGetSingle(context.User, out _))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public interface ICurrentOrganization
{
    string Id { get; }
}

public sealed class CurrentOrganization(IHttpContextAccessor accessor) : ICurrentOrganization
{
    public string Id
    {
        get
        {
            if (OrganizationScope.TryGetSingle(accessor.HttpContext?.User, out var id))
                return id!;
            // The authorization middleware must refuse invalid requests before a handler runs.
            throw new InvalidOperationException("An authorized organization is required.");
        }
    }
}
