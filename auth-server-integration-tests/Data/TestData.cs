using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleOidcOauth.Tests.Integration.Data
{
	/// <summary>A class containing constants and sample data to be used for testing/development purposes.</summary>
	internal static class TestData
	{
		// CONSTANTS / READ-ONLY
		/// <summary>Scope for an Identity Resource which provides basic informations about users.</summary>
		public const string ScopeIdentityResourceBasicUserInfo = "basic-user-info";


		/// <summary>Scope for an Identity Resource which provides confidential informations about users.</summary>
		public const string ScopeIdentityResourceConfidentialUserInfo = "confidential-user-info";


		/// <summary>Scope for an Api Resource which provides access to a "products" API.</summary>
		public const string ScopeApiResourceProducts = "products-api";


		/// <summary>Scope for an Api Resource which provides access to a "user management" API.</summary>
		public const string ScopeApiResourceUserManagement = "user-management-api";


		/// <summary>A fake plain text password to be used for the "Client Credentials Flow" client.</summary>
		public static readonly string PlainTextPasswordClientClientCredentialsFlow = "b22787170fdc45bfa22bbfc12e8776b2";


		/// <summary>A fake plain text password to be used for the "Resource Owner Password Flow" client.</summary>
		public static readonly string PlainTextPasswordClientResourceOwnerPasswordFlow = "930151c01fde4c7e9fe91ce56638e952";


		/// <summary>A fake plain text password to be used for the "Authorization Code (without PKCE)" client.</summary>
		public static readonly string PlainTextPasswordClientAuthorizationCodeFlowWithoutPkce = "e42b6cf8a482444385c62e5ee440f910";


		/// <summary>A fake plain text password to be used for the "Authorization Code (with PKCE)" client.</summary>
		public static readonly string PlainTextPasswordClientAuthorizationCodeFlowWithPkce = "74d8fb7b96dc4760846b432c7397f629";


		/// <summary>Stub collection of claims to be used as a placeholder for future improvements.</summary>
		/// <value>An empty collection of strings, which will be used as a placeholder for a future claims list.</value>
		public static readonly string[] EmptyClaimsCollection = new string[] {};


		/// <summary>An API Resource representing an API that deals with products.</summary>
		/// <returns>A pre-initialized <see cref="ApiResource" /> object representing an API that deals with products, and which can be used for testing/development purposes.</returns>
		public static readonly ApiResource ApiResourceProducts = new ApiResource(
			name: ScopeApiResourceProducts,
			displayName: "Products API",
			userClaims: EmptyClaimsCollection);


		/// <summary>An API Resource representing an API that deals with users management.</summary>
		/// <returns>A pre-initialized <see cref="ApiResource" /> object representing an API that deals with users management, and which can be used for testing/development purposes.</returns>
		public static readonly ApiResource ApiResourceUserManagement = new ApiResource(
			name: ScopeApiResourceUserManagement,
			displayName: "User management API",
			userClaims: EmptyClaimsCollection);


		/// <summary>An Identity Resource representing the basic information that can be obtained from a user.</summary>
		/// <returns>A pre-initialized <see cref="IdentityResource" /> object representing the basic claims of a user, and which can be used for testing/development purposes.</returns>
		public static readonly IdentityResource IdentityResourceBasicUserInfo = new IdentityResource(
			name: ScopeIdentityResourceBasicUserInfo,
			displayName: "Basic user information",
			userClaims: new [] {
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				JwtClaimTypes.GivenName,
			});


		/// <summary>An Identity Resource representing the private/confidential information that can be obtained from a user.</summary>
		/// <returns>A pre-initialized <see cref="IdentityResource" /> object representing the private/confidential claims of a user, and which can be used for testing/development purposes.</returns>
		public static readonly IdentityResource IdentityResourceConfidentialUserInfo = new IdentityResource(
			name: ScopeIdentityResourceConfidentialUserInfo,
			displayName: "Private/confidential user information",
			userClaims: new [] {
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				JwtClaimTypes.GivenName,
				JwtClaimTypes.Name,
				JwtClaimTypes.FamilyName,
				JwtClaimTypes.Email,
				JwtClaimTypes.EmailVerified,
				JwtClaimTypes.WebSite,
				JwtClaimTypes.Address,
			});


		/// <summary>Test user "Alice".</summary>
		/// <value>An object describing a mock user called "Alice".</value>
		public static readonly TestUser UserAlice = new TestUser{
			SubjectId = "818727",
			Username = "alice",
			Password = "alice123",
			Claims =
			{
				new Claim(JwtClaimTypes.Name, "Alice Smith"),
				new Claim(JwtClaimTypes.GivenName, "Alice"),
				new Claim(JwtClaimTypes.FamilyName, "Smith"),
				new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
				new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
				new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
				new Claim(JwtClaimTypes.PhoneNumber, "555-5555"),
				new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
			}
		};


		/// <summary>Test user "Bob".</summary>
		/// <value>An object describing a mock user called "Bob".</value>
		public static readonly TestUser UserBob = new TestUser{
			SubjectId = "88421113",
			Username = "bob",
			Password = "bob123",
			Claims =
			{
				new Claim(JwtClaimTypes.Name, "Bob Smith"),
				new Claim(JwtClaimTypes.GivenName, "Bob"),
				new Claim(JwtClaimTypes.FamilyName, "Smith"),
				new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
				new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean),
				new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
				new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
				new Claim("location", "somewhere")
			}
		};


		/// <summary>Test client configured for using the OAuth 2.0 Client Credentials flow.</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Client Credentials flow.</returns>
		public static readonly Client ClientClientCredentialsFlow = new Client()
		{
			ClientId = "client-client-credentials-flow",
			ClientName = "Client Credentials Flow Client",
			AllowedGrantTypes = GrantTypes.ClientCredentials,
			ClientSecrets = { new Secret(PlainTextPasswordClientClientCredentialsFlow.Sha256()) },
			AllowedScopes = { ScopeApiResourceUserManagement, ScopeApiResourceProducts },
			AllowedCorsOrigins = {
				"http://fake-cors-origin-5455bc181e3240b680b47a9c4479979f.com",
			},
			PostLogoutRedirectUris = {
				"http://fake-cors-origin-5455bc181e3240b680b47a9c4479979f.com/post-logout",
			},
			RedirectUris = {
				"http://fake-cors-origin-5455bc181e3240b680b47a9c4479979f.com/post-login",
			},
		};


		/// <summary>Test client configured for using the OAuth 2.0 Resource Owner Password flow.</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Resource Owner Password flow.</returns>
		public static readonly Client ClientResourceOwnerPasswordFlow = new Client()
		{
			ClientId = "client-resource-owner-password-flow",
			ClientName = "Resource Owner Password Flow Client",
			AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
			ClientSecrets = { new Secret(PlainTextPasswordClientResourceOwnerPasswordFlow.Sha256()) },
			AllowedScopes = { ScopeApiResourceUserManagement },
			RedirectUris = { "https://some-random-domain-07f1f04f60044334967205b97f01ac40.com/random-post-login-redirect-path" },
			PostLogoutRedirectUris = { "https://some-random-domain-07f1f04f60044334967205b97f01ac40.com/random-post-logout-redirect-path" },
		};


		/// <summary>Test client configured for using the OAuth 2.0 Authorization Code flow (without PKCE).</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Authorization Code flow (without PKCE).</returns>
        public static readonly Client ClientAuthorizationCodeFlowWithoutPkce = new Client
        {
            ClientId = "client-authorization-code-flow-no-pkce",
			ClientName = "Authorization Code Flow Client (Without PKCE)",
            ClientSecrets = { new Secret(PlainTextPasswordClientAuthorizationCodeFlowWithoutPkce.Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,
            RequireConsent = false,
            RequirePkce = false,

			AllowedCorsOrigins = { "https://localhost:5002" },
            RedirectUris = { "https://localhost:5002/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceProducts,
            }
        };


		/// <summary>Test client configured for using the OAuth 2.0 Authorization Code flow (with PKCE).</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Authorization Code flow (with PKCE).</returns>
        public static readonly Client ClientAuthorizationCodeFlowWithPkce = new Client
        {
            ClientId = "client-authorization-code-flow-with-pkce",
			ClientName = "Authorization Code Flow Client (With PKCE)",
            ClientSecrets = { new Secret(PlainTextPasswordClientAuthorizationCodeFlowWithPkce.Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,
            RequireConsent = false,
            RequirePkce = true,

			AllowedCorsOrigins = { "https://localhost:5003" },
            RedirectUris = { "https://localhost:5003/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceProducts,
            }
        };


		/// <summary>
		///     Test client configured for using the OAuth 2.0 Implicit Flow, wich is configured to be able to request
		///     both Access Tokens and Identity Tokens.
		/// </summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Implicit Flow.</returns>
        public static Client ClientImplicitFlowAccessAndIdTokens { get; } = new Client
		{
			ClientId = "client-implicit-flow-access-and-id-tokens",
			ClientName = "Implicit Flow Client (Access Tokens and ID Tokens)",
			AllowedGrantTypes = GrantTypes.Implicit,
			AllowAccessTokensViaBrowser = true,
			RequireClientSecret = false,

			RedirectUris =           { "http://localhost:5004/callback.html" },
			PostLogoutRedirectUris = { "http://localhost:5004/index.html" },
			AllowedCorsOrigins =     { "http://localhost:5004" },

			AllowedScopes =
			{
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceUserManagement,
			}
		};


		/// <summary>
		///     Test client configured for using the OAuth 2.0 Implicit Flow, wich is configured to be able to request
		///     only Access Tokens (not Identity Tokens, nor Authorization Codes).
		/// </summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Implicit Flow.</returns>
        public static Client ClientImplicitFlowAccessTokensOnly { get; } = new Client
		{
			ClientId = "client-implicit-flow-access-tokens-only",
			ClientName = "Implicit Flow Client (Access Tokens only)",
			AllowedGrantTypes = GrantTypes.Implicit,
			AllowAccessTokensViaBrowser = true,
			RequireClientSecret = false,

			RedirectUris =           { "http://localhost:5005/callback.html" },
			PostLogoutRedirectUris = { "http://localhost:5005/index.html" },
			AllowedCorsOrigins =     { "http://localhost:5005" },

			AllowedScopes =
			{
				ScopeApiResourceUserManagement,
			}
		};


		/// <summary>An API Scope representing an imaginary "Products API".</summary>
		public static ApiScope ApiScopeProductsApi = new ApiScope(ScopeApiResourceProducts, "Products API Scope");


		/// <summary>An API Scope representing an imaginary "User Management API".</summary>
		public static ApiScope ApiScopeUserManagementApi = new ApiScope(ScopeApiResourceUserManagement, "User Management API Scope");


		/// <summary>Collection of test Users.</summary>
		/// <value>A list of pre-initialized <see cref="TestUser" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<TestUser> SampleUsers = new TestUser[]
		{
			UserAlice,
			UserBob,
		};


		/// <summary>Collection of test Clients.</summary>
		/// <value>A list of pre-initialized <see cref="Client" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<Client> SampleClients = new Client[]
		{
			ClientClientCredentialsFlow,
			ClientResourceOwnerPasswordFlow,
			ClientAuthorizationCodeFlowWithPkce,
			ClientAuthorizationCodeFlowWithoutPkce,
			ClientImplicitFlowAccessTokensOnly,
			ClientImplicitFlowAccessAndIdTokens,
		};


		/// <summary>Collection of test API Scopes.</summary>
		/// <value>A list of pre-initialized <see cref="ApiScope"/> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<ApiScope> SampleApiScopes = new ApiScope[]
		{
			ApiScopeProductsApi,
			ApiScopeUserManagementApi,
		};


		/// <summary>Collection of test API Resources.</summary>
		/// <value>A list of pre-initialized <see cref="ApiResource" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<ApiResource> SampleApiResources = new ApiResource[]
		{
			ApiResourceProducts,
			ApiResourceUserManagement,
		};


		/// <summary>Collection of test Identity Resources.</summary>
		/// <value>A list of pre-initialized <see cref="IdentityResource" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<IdentityResource> SampleIdentityResources = new IdentityResource[]
		{
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
			IdentityResourceBasicUserInfo,
			IdentityResourceConfidentialUserInfo,
		};
	}
}