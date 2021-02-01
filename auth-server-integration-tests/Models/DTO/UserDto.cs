using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Test;

namespace SimpleOidcOauth.Tests.Integration.Models.DTO
{
	/// <summary>
	///     <para>
	///         A simplified <see cref="TestUser"/> version used to generate output for
	///         the <see cref="Controllers.TestDatabaseInitializerServiceController.GetAllRegisteredData"/> endpoint.
	///     </para>
	///     <para>
	///         This class is required to avoid serialization errors when returning JSON responses.
	///         Serialization errors include object cycles, trying to serialize properties whose types do not contain default
	///         constructors (e.g., <see cref="System.Security.Claims.Claim"/>), and others.
	///     </para>
	/// </summary>
	public class UserDto
	{
		// INSTANCE PROPERTIES
		/// <summary>Maps the <see cref="TestUser.SubjectId"/> property.</summary>
		public string SubjectId { get; set; }
		/// <summary>Maps the <see cref="TestUser.Username"/> property.</summary>
		public string Username { get; set; }
		/// <summary>Maps the <see cref="TestUser.Password"/> property.</summary>
		public string Password { get; set; }
		/// <summary>Maps the <see cref="TestUser.Claims"/> property.</summary>
		public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();






		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		public UserDto()
		{
		}


		/// <summary>Constructor.</summary>
		/// <param name="sourceUser">The source <see cref="TestUser"/> object to copy data from.</param>
		public UserDto(TestUser sourceUser)
		{
			SubjectId = sourceUser.SubjectId;
			Username = sourceUser.Username;
			Password = sourceUser.Password;
			foreach (var claim in sourceUser.Claims)
				Claims.Add(claim.Type, claim.Value);
		}


		/// <summary>Converts the <see cref="UserDto"/> into a <see cref="TestUser"/> instance.</summary>
		/// <returns>Returns the corresponding <see cref="TestUser"/> instance.</returns>
		public TestUser MakeTestUser()
		{
			var result = new TestUser()
			{
				SubjectId = this.SubjectId,
				Username = this.Username,
				Password = this.Password,
			};

			foreach (var claim in Claims)
				result.Claims.Add(new Claim(claim.Key, claim.Value));

			return result;
		}
	}
}