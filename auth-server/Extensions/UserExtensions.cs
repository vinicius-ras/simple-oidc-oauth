using AutoMapper;
using IdentityModel;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using SimpleOidcOauth.Data.Security;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleOidcOauth.Extensions
{
	/// <summary>Extension methods for the types <see cref="TestUser"/> and <see cref="ApplicationUser"/></summary>
	public static class UserExtensions
	{
		/// <summary>
		///     Converts an IdentityServer <see cref="TestUser" /> object to the
		///     corresponding ASP.NET Core Identity user model representation, as used by this application.
		/// </summary>
		/// <param name="testUser">The <see cref="TestUser" /> object to be converted.</param>
		/// <param name="mapper">Reference to an <see cref="IMapper" /> object to convert between entities.</param>
		/// <param name="mapper">An <see cref="UserManager{TUser}" /> object to calculate special values (e.g., the user's password hash).</param>
		/// <returns>Returns an <see cref="ApplicationUser" /> object containing the data which was extracted from the input object.</returns>
		public static ApplicationUser ConvertToApplicationUser(this TestUser testUser, IMapper mapper, UserManager<ApplicationUser> userManager)
		{
			var result = mapper.Map<ApplicationUser>(testUser);

			result.NormalizedEmail = userManager.NormalizeEmail(result.Email);
			result.NormalizedUserName = userManager.NormalizeName(result.UserName);
			result.PasswordHash = userManager.PasswordHasher.HashPassword(result, testUser.Password);
			return result;
		}


		/// <summary>Converts an <see cref="ApplicationUser" /> object to the corresponding <see cref="TestUser"/> representation.</summary>
		/// <param name="applicationUser">The <see cref="ApplicationUser" /> object to be converted.</param>
		/// <returns>Returns a <see cref="TestUser" /> object containing the data which was extracted from the input object.</returns>
		public static TestUser ConvertToTestUser(this ApplicationUser applicationUser)
		{
			var result = new TestUser()
			{
				Username = applicationUser.UserName,
				Claims = new List<Claim>(),
			};

			if (string.IsNullOrWhiteSpace(applicationUser.Email) == false)
				result.Claims.Add(new Claim(JwtClaimTypes.Email, applicationUser.Email));
			result.Claims.Add(new Claim(JwtClaimTypes.EmailVerified, applicationUser.EmailConfirmed.ToString().ToLower()));
			if (string.IsNullOrWhiteSpace(applicationUser.PhoneNumber) == false)
				result.Claims.Add(new Claim(JwtClaimTypes.PhoneNumber, applicationUser.PhoneNumber));

			return result;
		}
	}
}