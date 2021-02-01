using IdentityModel;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleOidcOauth.Extensions
{
	/// <summary>Extension methods for the types <see cref="TestUser"/> and <see cref="IdentityUser"/></summary>
	public static class UserExtensions
	{
		/// <summary>
		///     Converts an IdentityServer <see cref="TestUser" /> object to the
		///     corresponding ASP.NET Core Identity user model representation, as used by this application.
		/// </summary>
		/// <param name="testUser">The <see cref="TestUser" /> object to be converted.</param>
		/// <returns>Returns an <see cref="IdentityUser" /> object containing the data which was extracted from the input object.</returns>
		public static IdentityUser ConvertToIdentityUser(this TestUser testUser, UserManager<IdentityUser> userManager)
		{
			var result = new IdentityUser()
			{
				UserName = testUser.Username,
				Email = testUser.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value,
				EmailConfirmed = testUser.Claims
					.FirstOrDefault(c => c.Type == JwtClaimTypes.EmailVerified)?.Value
					switch {
						null => false,
						string claimValue => bool.Parse(claimValue),
					},
				PhoneNumber = testUser.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.PhoneNumber)?.Value,
			};

			result.NormalizedEmail = userManager.NormalizeEmail(result.Email);
			result.NormalizedUserName = userManager.NormalizeName(result.UserName);
			result.PasswordHash = userManager.PasswordHasher.HashPassword(result, testUser.Password);
			return result;
		}


		/// <summary>Converts an <see cref="IdentityUser" /> object to the corresponding <see cref="TestUser"/> representation.</summary>
		/// <param name="identityUser">The <see cref="IdentityUser" /> object to be converted.</param>
		/// <returns>Returns a <see cref="TestUser" /> object containing the data which was extracted from the input object.</returns>
		public static TestUser ConvertToTestUser(this IdentityUser identityUser)
		{
			var result = new TestUser()
			{
				Username = identityUser.UserName,
				Claims = new List<Claim>(),
			};

			if (string.IsNullOrWhiteSpace(identityUser.Email) == false)
				result.Claims.Add(new Claim(JwtClaimTypes.Email, identityUser.Email));
			result.Claims.Add(new Claim(JwtClaimTypes.EmailVerified, identityUser.EmailConfirmed.ToString().ToLower()));
			if (string.IsNullOrWhiteSpace(identityUser.PhoneNumber) == false)
				result.Claims.Add(new Claim(JwtClaimTypes.PhoneNumber, identityUser.PhoneNumber));

			return result;
		}
	}
}