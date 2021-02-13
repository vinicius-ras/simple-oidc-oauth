using AutoMapper;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
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
				.ReverseMap();
		}
	}
}