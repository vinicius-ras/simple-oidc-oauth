using AutoMapper;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using SimpleOidcOauth.Data.Serialization;
using System.Linq;
using System.Security.Claims;

namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>An AutoMapper profile, defining rules for mapping between types.</summary>
	public class AutoMapperProfile : Profile
	{
		/// <summary>Constructor.</summary>
		public AutoMapperProfile()
		{
			CreateMap<Claim, SerializableClaim>()
				.ReverseMap();
			CreateMap<Secret, SerializableSecret>()
				.ReverseMap();
			CreateMap<Client, SerializableClient>()
				.ReverseMap();
			CreateMap<Resource, SerializableResource>()
				.ReverseMap();
			CreateMap<ApiScope, SerializableApiScope>()
				.IncludeBase<Resource, SerializableResource>()
				.ReverseMap();
			CreateMap<ApiResource, SerializableApiResource>()
				.IncludeBase<Resource, SerializableResource>()
				.ReverseMap();
			CreateMap<IdentityResource, SerializableIdentityResource>()
				.IncludeBase<Resource, SerializableResource>()
				.ReverseMap();
			CreateMap<TestUser, SerializableTestUser>()
				.ForMember(serializableTestUser => serializableTestUser.Email, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Email).Value))
				.ForMember(serializableTestUser => serializableTestUser.EmailConfirmed, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.EmailVerified).Value))
				.ForMember(serializableTestUser => serializableTestUser.Claims, opt => opt.MapFrom(testUser => testUser.Claims))
				.ReverseMap();
			CreateMap<TestUser, IdentityUser>()
				.ForMember(identityUser => identityUser.AccessFailedCount, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.ConcurrencyStamp, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.Id, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.LockoutEnabled, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.LockoutEnd, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.NormalizedEmail, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.NormalizedUserName, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.PasswordHash, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.SecurityStamp, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.TwoFactorEnabled, opt => opt.Ignore())
				.ForMember(identityUser => identityUser.Email, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Email).Value))
				.ForMember(identityUser => identityUser.EmailConfirmed, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.EmailVerified).Value))
				.ForMember(identityUser => identityUser.PhoneNumber, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.PhoneNumber).Value))
				.ForMember(identityUser => identityUser.PhoneNumberConfirmed, opt => opt.MapFrom(testUser => testUser.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.PhoneNumberVerified).Value))
				.ForMember(identityUser => identityUser.UserName, opt => opt.MapFrom(testUser => testUser.Username))
				.ReverseMap();
		}
	}
}