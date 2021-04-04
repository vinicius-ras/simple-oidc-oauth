using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace SimpleOidcOauth.Security.Authorization.Requirements
{
	/// <summary>An Authorization Requirement where the user must have at least one of the claims in a given set of claims.</summary>
	class AtLeastOneClaimAuthorizationRequirement : IAuthorizationRequirement
	{
		// INSTANCE PROPERTIES
		/// <param name="claimsSet">The set of claims, from which the user must have at least one of for the Authorization to be granted.</param>
		public IEnumerable<string> AcceptableClaimTypes { get; }





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="claimsSet">The set of claims, from which the user must have at least one of for the Authorization to be granted.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="claimsSet"/> is set to <c>null</c>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="claimsSet"/> is an empty collection, or a collection containing only one element.</exception>
		public AtLeastOneClaimAuthorizationRequirement(params string[] claimsSet)
		{
			// Runtime error checking
			if (claimsSet == null)
				throw new ArgumentNullException(
					nameof(claimsSet),
					$@"Authorization requirement ""{nameof(AtLeastOneClaimAuthorizationRequirement)}"" cannot be instantiated with a null collection of claims."
				);
			else if (claimsSet.Length <= 0)
				throw new ArgumentException(
					$@"Authorization requirement ""{nameof(AtLeastOneClaimAuthorizationRequirement)}"" cannot be instantiated with an empty collection of claims.",
					nameof(claimsSet)
				);
			else if (claimsSet.Length == 1)
				throw new ArgumentException(
					$@"Authorization requirement ""{nameof(AtLeastOneClaimAuthorizationRequirement)}"" cannot be instantiated with a collection of claims containing a single claim (it requires 2 or more claim names).",
					nameof(claimsSet)
				);

			// Initialization
			AcceptableClaimTypes = claimsSet;
		}
	}
}