using AutoMapper;
using IdentityModel;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Test;
using SimpleOidcOauth.Data.Security;
using SimpleOidcOauth.Data.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Is4HashExtensions = IdentityServer4.Models.HashExtensions;

namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>An AutoMapper profile, defining rules for mapping between types.</summary>
	public class AutoMapperProfile : Profile
	{
		/// <summary>Constructor.</summary>
		public AutoMapperProfile()
		{
			// Mappings for Claims
			CreateMap<Claim, SerializableClaim>()
				.ReverseMap();


			// Mappings for Secrets
			CreateMap<Secret, SerializableSecret>()
				.Include<ClientSecret, SerializableSecret>()
				.Include<ApiResourceSecret, SerializableSecret>()
				.ForMember(serializableSecret => serializableSecret.IsValueHashed, opts => opts.MapFrom(_ => true))
				.ForMember(serializableSecret => serializableSecret.Value, opts => opts.MapFrom(_ => (string)null))
				.ForMember(serializableSecret => serializableSecret.DatabaseId, opts => opts.MapFrom(secret => secret.Id))
				.ReverseMap()
				.ForMember(secret => secret.Value, opts => opts.MapFrom(serializableSecret => serializableSecret.IsValueHashed == true
					? serializableSecret.Value
					: Is4HashExtensions.Sha256(serializableSecret.Value)));
			CreateMap<ClientSecret, SerializableSecret>()
				.ReverseMap()
				.ForMember(clientSecret => clientSecret.Client, opts => opts.Ignore());
			CreateMap<ApiResourceSecret, SerializableSecret>()
				.ReverseMap()
				.ForMember(apiResourceSecret => apiResourceSecret.ApiResource, opts => opts.Ignore());


			// Mappings for Client Applications
			CreateMap<Client, SerializableClient>()
				.ForMember(serializableClient => serializableClient.ClientDatabaseId, opts => opts.MapFrom(client => client.Id))
				.ForMember(serializableClient => serializableClient.AllowedGrantTypes, opts => opts.MapFrom(client => client.AllowedGrantTypes.Select(grantType => grantType.GrantType)))
				.ForMember(serializableClient => serializableClient.RedirectUris, opts => opts.MapFrom(client => client.RedirectUris.Select(redirectUri => redirectUri.RedirectUri)))
				.ForMember(serializableClient => serializableClient.PostLogoutRedirectUris, opts => opts.MapFrom(client => client.PostLogoutRedirectUris.Select(postLogoutRedirectUri => postLogoutRedirectUri.PostLogoutRedirectUri)))
				.ForMember(serializableClient => serializableClient.AllowedCorsOrigins, opts => opts.MapFrom(client => client.AllowedCorsOrigins.Select(corsOrigin => corsOrigin.Origin)))
				.ForMember(serializableClient => serializableClient.AllowedScopes, opts => opts.MapFrom(client => client.AllowedScopes.Select(scope => scope.Scope)))
				.ForMember(serializableClient => serializableClient.ClientSecrets, opts => opts.MapFrom(client => client.ClientSecrets))
				.ReverseMap()
				.ForMember(client => client.Id, opts => opts.MapFrom(serializableSecret => serializableSecret.ClientDatabaseId))
				.ForMember(client => client.IdentityProviderRestrictions, opts => opts.Ignore())
				.ForMember(client => client.Claims, opts => opts.Ignore())
				.ForMember(client => client.Properties, opts => opts.Ignore())
				.ForMember(client => client.AllowedGrantTypes, opts => opts.MapFrom(serializableClient =>
					serializableClient.AllowedGrantTypes.Select(grantType => new ClientGrantType {
						ClientId = serializableClient.ClientDatabaseId ?? 0,
						GrantType = grantType,
					}).ToList()
				))
				.ForMember(client => client.AllowedCorsOrigins, opts => opts.MapFrom(serializableClient =>
					serializableClient.AllowedCorsOrigins.Select(corsOrigin => new ClientCorsOrigin {
						ClientId = serializableClient.ClientDatabaseId ?? 0,
						Origin = corsOrigin,
					}).ToList()
				))
				.ForMember(client => client.RedirectUris, opts => opts.MapFrom(serializableClient =>
					serializableClient.RedirectUris.Select(redirectUri => new ClientRedirectUri {
						ClientId = serializableClient.ClientDatabaseId ?? 0,
						RedirectUri = redirectUri,
					}).ToList()
				))
				.ForMember(client => client.PostLogoutRedirectUris, opts => opts.MapFrom(serializableClient =>
					serializableClient.PostLogoutRedirectUris.Select(postLogoutRedirectUri => new ClientPostLogoutRedirectUri {
						ClientId = serializableClient.ClientDatabaseId ?? 0,
						PostLogoutRedirectUri = postLogoutRedirectUri,
					}).ToList()
				))
				.ForMember(client => client.AllowedScopes, opts => opts.MapFrom(serializableClient =>
					serializableClient.AllowedScopes.Select(scope => new ClientScope {
						ClientId = serializableClient.ClientDatabaseId ?? 0,
						Scope = scope,
					}).ToList()
				))
				;


			// Mappings for Identity Server's Resources (API Scopes, API Resources, Identity Resources).
			CreateMap<ApiResource, SerializableResource>()
				.ForMember(serializableResource => serializableResource.ResourceDatabaseId, opts => opts.MapFrom(apiResource => apiResource.Id))
				.ForMember(serializableResource => serializableResource.UserClaims, opts => opts.MapFrom(apiResource => apiResource.UserClaims.Select(claim => claim.Type)))
				.ForMember(serializableResource => serializableResource.Properties, opts => opts.MapFrom(apiResource => new Dictionary<string,string>(apiResource.Properties.Select(property => KeyValuePair.Create(property.Key, property.Value)))))
				.ReverseMap()
				.ForMember(apiResource => apiResource.Id, opts => opts.Ignore())
				.ForMember(apiResource => apiResource.UserClaims, opts => opts.MapFrom(serializableResource => serializableResource.UserClaims.Select(claim => new ApiResourceClaim
					{
						ApiResourceId = serializableResource.ResourceDatabaseId ?? 0,
						Type = claim,
					}).ToList()
				))
				.ForMember(apiResource => apiResource.Properties, opts => opts.MapFrom(serializableResource => serializableResource.Properties.Select(property => new ApiResourceProperty
					{
						ApiResourceId = serializableResource.ResourceDatabaseId ?? 0,
						Key = property.Key,
						Value = property.Value,
					}).ToList()
				));
			CreateMap<ApiResource, SerializableApiResource>()
				.IncludeBase<ApiResource, SerializableResource>()
				.ForMember(serializableApiResource => serializableApiResource.ApiSecrets, opts => opts.MapFrom(apiResource => apiResource.Secrets))
				.ForMember(serializableApiResource => serializableApiResource.Scopes, opts => opts.MapFrom(apiResource => apiResource.Scopes.Select(scope => scope.Scope)))
				.ReverseMap()
				.ForMember(apiResource => apiResource.Scopes, opts => opts.MapFrom(serializableApiResource => serializableApiResource.Scopes.Select(scope => new ApiResourceScope
					{
						ApiResourceId = serializableApiResource.ResourceDatabaseId ?? 0,
						Scope = scope,
					}).ToList()
				));

			CreateMap<ApiScope, SerializableResource>()
				.ForMember(serializableResource => serializableResource.ResourceDatabaseId, opts => opts.MapFrom(apiScope => apiScope.Id))
				.ForMember(serializableApiScope => serializableApiScope.UserClaims, opts => opts.MapFrom(apiScope => apiScope.UserClaims.Select(claim => claim.Type)))
				.ForMember(serializableResource => serializableResource.Properties, opts => opts.MapFrom(apiScope => new Dictionary<string,string>(apiScope.Properties.Select(property => KeyValuePair.Create(property.Key, property.Value)))))
				.ReverseMap()
				.ForMember(apiScope => apiScope.UserClaims, opts => opts.MapFrom(serializableResource => serializableResource.UserClaims.Select(claim => new ApiScopeClaim
					{
						ScopeId = serializableResource.ResourceDatabaseId ?? 0,
						Type = claim,
					}).ToList()
				))
				.ForMember(apiScope => apiScope.Properties, opts => opts.MapFrom(serializableResource => serializableResource.Properties.Select(property => new ApiScopeProperty
					{
						ScopeId = serializableResource.ResourceDatabaseId ?? 0,
						Key = property.Key,
						Value = property.Value,
					}).ToList()
				));
			CreateMap<ApiScope, SerializableApiScope>()
				.IncludeBase<ApiScope, SerializableResource>()
				.ReverseMap();

			CreateMap<IdentityResource, SerializableResource>()
				.ForMember(serializableResource => serializableResource.ResourceDatabaseId, opts => opts.MapFrom(identityResource => identityResource.Id))
				.ForMember(serializableResource => serializableResource.UserClaims, opts => opts.MapFrom(identityResource => identityResource.UserClaims.Select(claim => claim.Type)))
				.ForMember(serializableResource => serializableResource.Properties, opts => opts.MapFrom(identityResource => new Dictionary<string,string>(identityResource.Properties.Select(property => KeyValuePair.Create(property.Key, property.Value)))))
				.ReverseMap()
				.ForMember(identityResource => identityResource.Id, opts => opts.Ignore())
				.ForMember(identityResource => identityResource.UserClaims, opts => opts.MapFrom(serializableResource => serializableResource.UserClaims.Select(claim => new IdentityResourceClaim
					{
						IdentityResourceId = serializableResource.ResourceDatabaseId ?? 0,
						Type = claim,
					}).ToList()
				))
				.ForMember(identityResource => identityResource.Properties, opts => opts.MapFrom(serializableResource => serializableResource.Properties.Select(property => new IdentityResourceProperty
					{
						IdentityResourceId = serializableResource.ResourceDatabaseId ?? 0,
						Key = property.Key,
						Value = property.Value,
					}).ToList()
				));
			CreateMap<IdentityResource, SerializableIdentityResource>()
				.IncludeBase<IdentityResource, SerializableResource>()
				.ReverseMap();


			// Mappings for Users data
			CreateMap<TestUser, SerializableTestUser>()
				.ForMember(serializableTestUser => serializableTestUser.Claims, opt => opt.MapFrom(testUser => testUser.Claims))
				.ReverseMap();
			CreateMap<TestUser, ApplicationUser>()
				.ForMember(applicationUser => applicationUser.AccessFailedCount, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.ConcurrencyStamp, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.Id, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.LockoutEnabled, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.LockoutEnd, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.NormalizedEmail, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.NormalizedUserName, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.PasswordHash, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.SecurityStamp, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.TwoFactorEnabled, opt => opt.Ignore())
				.ForMember(applicationUser => applicationUser.Email, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Email).Value))
				.ForMember(applicationUser => applicationUser.EmailConfirmed, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.EmailVerified).Value))
				.ForMember(applicationUser => applicationUser.PhoneNumber, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.PhoneNumber).Value))
				.ForMember(applicationUser => applicationUser.PhoneNumberConfirmed, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.PhoneNumberVerified).Value))
				.ForMember(applicationUser => applicationUser.UserName, opt => opt.MapFrom(testUser => testUser.Username))
				.ReverseMap();
		}
	}
}