using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IdentityModel;
using IdentityServer4;
using SimpleOidcOauth.Data.Serialization;
using Xunit;
using Is4HashExtensions = IdentityServer4.Models.HashExtensions;

namespace SimpleOidcOauth.Tests.Unit.Data.Serialization
{
	/// <summary>Unit tests for the <see cref="SerializableClient"/> class.</summary>
	public class SerializableClientTests
	{
		// STATIC METHODS
		/// <summary>Retrieves an instance of the <see cref="SerializableClient"/> class, prefilled with valid data for the tests.</summary>
		/// <returns>Returns a new instance of a <see cref="SerializableClient"/>, configured with valid data.</returns>
		private SerializableClient InstantiateValidSerializableClient() => new SerializableClient
		{
			ClientName = "client-2af8a279423a43f8b39183f7f4788bae",
			AllowAccessTokensViaBrowser = true,
			AllowedCorsOrigins = new [] {
				"http://valid-cors-origin-358102f663384047a6bcb0ac24ace0bf.com",
				"https://valid-cors-origin-546097caed384ce59d7757dfee8aefa4.com.br",
			},
			AllowedGrantTypes = new [] {
				OidcConstants.GrantTypes.ClientCredentials,
				OidcConstants.GrantTypes.Implicit,
			},
			AllowedScopes = new [] {
				OidcConstants.StandardScopes.OpenId,
				OidcConstants.StandardScopes.Profile,
				"random-scope-6e0ac6e2df3b4753a6c3ddce8f8c82c2",
			},
			ClientSecrets = new [] {
				new SerializableSecret { Type = IdentityServerConstants.SecretTypes.SharedSecret, Value = Is4HashExtensions.Sha256("my-shared-secret-840dada4e56c4417b6d307efe6148d69") },
			},
			PostLogoutRedirectUris = new [] {
				"http://valid-post-logout-redirect-uri-7db192b1d75e4a0da751f394f92b2839.com",
				"https://valid-post-logout-redirect-uri-3627bc2794874f4d8c9f1c004cc99738.com.br",
			},
			RedirectUris = new [] {
				"http://valid-post-redirect-uri-c81dd937c33148aaa3e8c0c2cb0e8c5d.com",
				"https://valid-post-redirect-uri-cbd9758db8fe4ab7b3fede088b377d8d.com.ca",
			},
			RequireClientSecret = true,
			RequireConsent = false,
			RequirePkce = false,
		};





		// TESTS
		[Fact]
		public void Validate_AllValidData_ReturnsSuccess()
		{
			var clientData = InstantiateValidSerializableClient();
			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.True(isValid);
			Assert.Empty(errorsCollection);
		}


		[Fact]
		public void Validate_AllValidDataWithNullCorsOrigins_ReturnsSuccess()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedCorsOrigins = null;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.True(isValid);
			Assert.Empty(errorsCollection);
		}


		[Fact]
		public void Validate_AllValidDataWithEmptyCorsOrigins_ReturnsSuccess()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedCorsOrigins = new string[0];

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.True(isValid);
			Assert.Empty(errorsCollection);
		}


		[Theory]
		[InlineData("http://valid-origin-510f3ee958804edaa141288cc40c4a42.com")]
		[InlineData("http://valid-origin-91a78b4f13ce415bb70f91b6950037c1.com.br", "https://valid-origin-14f448e1336e42e6be120f11e954d0a4.gov.br")]
		public void Validate_AllValidDataWithValidCorsOrigins_ReturnsSuccess(params string[] corsOrigins)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedCorsOrigins = corsOrigins;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.True(isValid);
			Assert.Empty(errorsCollection);
		}


		[Theory]
		[InlineData("http://valid-origin-510f3ee958804edaa141288cc40c4a42.com/")]
		[InlineData("http://valid-origin-5db39a71d6174691bba4ab4ad842423e.com.br/some/path")]
		[InlineData("http://valid-origin-7dfcf200e82a4ba19d9536e594a16c98.com.br/some/path?with-query=123")]
		[InlineData("http://valid-origin-7d79896f6a8a4341b23a5c3015b62f07.com.br/some/path?with-query=123#with-fragment")]
		[InlineData("http://valid-origin-b3895695c0814bfabc0c508e38a916db.com.br?with-query=123#with-fragment")]
		[InlineData("http://valid-origin-9f8900fb761c4fd4b08e7cb3dd616c85.com.br?with-query=123")]
		[InlineData("http://valid-origin-c22636e51c85426c928d18f4372b34f2.com.br#with-fragment")]
		[InlineData("invalid-origin-0fe1729b1e654b7592fb4bd2003237d4.com")]
		[InlineData("/some/relative/path")]
		[InlineData("ftp://invalid-schema-b6402ffe55604c6c9f4a708d60f5dd50.ca")]
		[InlineData("file:///invalid-schema-e3a1414b53d94bc5a549add92270c06c.gov.fr")]
		public void Validate_AllValidDataWithInvalidCorsOrigins_ReturnsFailure(params string[] corsOrigins)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedCorsOrigins = corsOrigins;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.AllowedCorsOrigins));
		}


		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("     ")]
		[InlineData("\n\n\n")]
		[InlineData("\t\t\t")]
		[InlineData("\n \t \n \t")]
		public void Validate_AllValidDataWithNullOrEmptyClientName_ReturnsFailure(string clientName)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.ClientName = clientName;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.ClientName));
		}


		[Fact]
		public void Validate_AllValidDataWithNullAllowedGrantTypes_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedGrantTypes = null;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.AllowedGrantTypes));
		}


		[Fact]
		public void Validate_AllValidDataWithEmptyAllowedGrantTypes_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedGrantTypes = new string[0];

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.AllowedGrantTypes));
		}


		[Theory]
		[InlineData("invalid_grant_type_2398c3abdf344a3490bb709d499016c5")]
		[InlineData(OidcConstants.GrantTypes.AuthorizationCode, "invalid_grant_type_497171c86b114a5ea2cc38199aebd999")]
		public void Validate_AllValidDataContainingUnkownGrantTypes_ReturnsFailure(params string[] grantTypes)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedGrantTypes = grantTypes;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.AllowedGrantTypes));
		}


		[Theory]
		[InlineData(OidcConstants.GrantTypes.AuthorizationCode, OidcConstants.GrantTypes.AuthorizationCode)]
		[InlineData(OidcConstants.GrantTypes.ClientCredentials, OidcConstants.GrantTypes.AuthorizationCode, OidcConstants.GrantTypes.ClientCredentials)]
		[InlineData(OidcConstants.GrantTypes.ClientCredentials, OidcConstants.GrantTypes.AuthorizationCode, OidcConstants.GrantTypes.AuthorizationCode, OidcConstants.GrantTypes.ClientCredentials)]
		public void Validate_AllValidDataContainingDuplicateGrantTypes_ReturnsFailure(params string[] grantTypes)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.AllowedGrantTypes = grantTypes;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.AllowedGrantTypes));
		}


		[Fact]
		public void Validate_ClientConfigureToRequireClientSecretWithNullClientSecrets_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.RequireClientSecret = true;
			clientData.ClientSecrets = null;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.ClientSecrets));
		}


		[Fact]
		public void Validate_ClientConfigureToRequireClientSecretWithEmptyClientSecrets_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.RequireClientSecret = true;
			clientData.ClientSecrets = new List<SerializableSecret>();

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.ClientSecrets));
		}


		[Fact]
		public void Validate_ClientSecretsWithNullReferences_ReturnsFailure()
		{
			var plainTextSecrets = new [] { "my-secret-40e98a8d62a841bcb9d58c35fa63e01e", null, "my-secret-0e5fa86d7a234704ba791a6d9765e4a7" };

			var clientData = InstantiateValidSerializableClient();
			clientData.ClientSecrets = plainTextSecrets
				.Select(plainTextSecret => plainTextSecret == null
					? null
					: new SerializableSecret {
						Type = IdentityServerConstants.SecretTypes.SharedSecret,
						Value = Is4HashExtensions.Sha256(plainTextSecret),
					})
				.ToList();
			string expectedErrorMemberName = $"{nameof(clientData.ClientSecrets)}[1]";

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == expectedErrorMemberName);
		}


		[Fact]
		public void Validate_ClientSecretsWithNullValuesAndNotHashed_ReturnsFailure()
		{
			var plainTextSecrets = new [] { "my-secret-40e98a8d62a841bcb9d58c35fa63e01e", null, "my-secret-0e5fa86d7a234704ba791a6d9765e4a7" };

			var clientData = InstantiateValidSerializableClient();
			clientData.ClientSecrets = plainTextSecrets
				.Select(plainTextSecret => new SerializableSecret {
					Type = IdentityServerConstants.SecretTypes.SharedSecret,
					Value = plainTextSecret != null
						? Is4HashExtensions.Sha256(plainTextSecret)
						: null,
					IsValueHashed = false,
				})
				.ToList();
			string expectedErrorMemberName = $"{nameof(clientData.ClientSecrets)}[1].{nameof(SerializableSecret.Value)}";

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == expectedErrorMemberName);
		}


		[Fact]
		public void Validate_ClientSecretsWithNullType_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.ClientSecrets = new [] {
				new SerializableSecret {
					Type = IdentityServerConstants.SecretTypes.SharedSecret,
					Value = Is4HashExtensions.Sha256("my-secret-da89bba38e214e9c9ffcc645b65ebd0d"),
				},
				new SerializableSecret {
					Type = null,
					Value = Is4HashExtensions.Sha256("my-secret-ad8f32f132804558831008b7ea0d22d5"),
				},
				new SerializableSecret {
					Type = IdentityServerConstants.SecretTypes.SharedSecret,
					Value = Is4HashExtensions.Sha256("my-secret-df0ffa444ee346fc9ba39a76f30a56d0"),
				},
			};
			string expectedErrorMemberName = $"{nameof(clientData.ClientSecrets)}[1].{nameof(SerializableSecret.Type)}";

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == expectedErrorMemberName);
		}


		[Fact]
		public void Validate_ClientWithNullRedirectUrls_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.RedirectUris = null;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.RedirectUris));
		}


		[Fact]
		public void Validate_ClientWithEmptyRedirectUrls_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.RedirectUris = new string[0];

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.RedirectUris));
		}


		[Theory]
		[InlineData("http://localhost:1234/abc/def")]
		[InlineData("https://www.my-random-domain-ec6b7700de284c72ad4795da61476a65.com.br/target-redirect-path")]
		public void Validate_ClientWithValidRedirectUrls_ReturnsSuccess(params string[] redirectUrls)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.RedirectUris = redirectUrls;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.True(isValid);
			Assert.Empty(errorsCollection);
		}


		[Theory]
		[InlineData("localhost:1234/abc/def")]
		[InlineData("www.my-random-domain-b6c093b2466d4b038613db01027dacd2.com.br/target-redirect-path")]
		[InlineData("/some/relative/path")]
		[InlineData("some/other/relative/path")]
		public void Validate_ClientWithInvalidRedirectUrls_ReturnsFailure(params string[] redirectUrls)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.RedirectUris = redirectUrls;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.RedirectUris));
		}


		[Fact]
		public void Validate_ClientWithNullPostLogoutRedirectUrls_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.PostLogoutRedirectUris = null;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.PostLogoutRedirectUris));
		}


		[Fact]
		public void Validate_ClientWithEmptyPostLogoutRedirectUrls_ReturnsFailure()
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.PostLogoutRedirectUris = new string[0];

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.PostLogoutRedirectUris));
		}


		[Theory]
		[InlineData("http://localhost:1234/abc/def")]
		[InlineData("https://www.my-random-domain-f54bce8e865c4f58a976a9eec6476069.com.br/target-redirect-path")]
		public void Validate_ClientWithValidPostLogoutRedirectUrls_ReturnsSuccess(params string[] redirectUrls)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.PostLogoutRedirectUris = redirectUrls;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.True(isValid);
			Assert.Empty(errorsCollection);
		}


		[Theory]
		[InlineData("localhost:1234/abc/def")]
		[InlineData("www.my-random-domain-89eb58c8b8cc4b948ff98a4f19c15a4d.com.br/target-redirect-path")]
		[InlineData("/some/relative/path")]
		[InlineData("some/other/relative/path")]
		public void Validate_ClientWithInvalidPostLogoutRedirectUrls_ReturnsFailure(params string[] redirectUrls)
		{
			var clientData = InstantiateValidSerializableClient();
			clientData.PostLogoutRedirectUris = redirectUrls;

			var validationContext = new ValidationContext(clientData);
			var errorsCollection = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(clientData, validationContext, errorsCollection, true);

			Assert.False(isValid);
			Assert.NotEmpty(errorsCollection);
			Assert.Contains(errorsCollection, error => error.MemberNames.Count() == 1 && error.MemberNames.First() == nameof(clientData.PostLogoutRedirectUris));
		}
	}
}