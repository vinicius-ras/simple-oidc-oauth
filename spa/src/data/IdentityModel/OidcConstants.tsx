/** A copy of the OAuth/OIDC Grant Types provided by the IdentityModel library. */
export abstract class GrantTypes
{
	public static readonly Password = "password";
	public static readonly AuthorizationCode = "authorization_code";
	public static readonly ClientCredentials = "client_credentials";
	public static readonly RefreshToken = "refresh_token";
	public static readonly Implicit = "implicit";
	public static readonly Saml2Bearer = "urn:ietf:params:oauth:grant-type:saml2-bearer";
	public static readonly JwtBearer = "urn:ietf:params:oauth:grant-type:jwt-bearer";
	public static readonly DeviceCode = "urn:ietf:params:oauth:grant-type:device_code";
	public static readonly TokenExchange = "urn:ietf:params:oauth:grant-type:token-exchange";
}
