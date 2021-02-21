using AutoMapper;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using SimpleOidcOauth.Data.Security;
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