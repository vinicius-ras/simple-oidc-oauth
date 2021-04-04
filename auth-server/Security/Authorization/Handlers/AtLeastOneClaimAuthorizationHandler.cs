using Microsoft.AspNetCore.Authorization;
using SimpleOidcOauth.Security.Authorization.Requirements;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Security.Authorization.Handlers
{
	/// <summary>
	///     An Authorization Handler that processes Authorization Requirements of type <see cref="AtLeastOneClaimAuthorizationRequirement"/>.
	///     This handler simply checks if the current <see cref="ClaimsPrincipal"/> contains one of the claims specified in
	///     the <see cref="AtLeastOneClaimAuthorizationRequirement.AcceptableClaimTypes"/> collection.
	/// </summary>
	class AtLeastOneClaimAuthorizationHandler : AuthorizationHandler<AtLeastOneClaimAuthorizationRequirement>
	{
		// ABSTRACT CLASS IMPLEMENTATION: AuthorizationHandler<AtLeastOneClaimRequirement>
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AtLeastOneClaimAuthorizationRequirement requirement)
		{
			if (context.User?.HasClaim(c => requirement.AcceptableClaimTypes.Contains(c.Type)) == true)
				context.Succeed(requirement);
			return Task.CompletedTask;
		}
	}
}