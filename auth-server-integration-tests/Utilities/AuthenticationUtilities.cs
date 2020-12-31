using IdentityModel;
using IdentityModel.Client;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using SimpleOidcOauth.Models;
using SimpleOidcOauth.Tests.Integration.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleOidcOauth.Tests.Integration.Utilities
{
	/// <summary>Utility class with methods make it easier to deal with user authentication matters during the tests.</summary>
	static class AuthenticationUtilities
	{
		// CONSTANTS
		/// <summary>A fake PKCE Code Verifier to be used during tests, whenever necessary.</summary>
		private const string FakePkceCodeVerifier = "ac325f2bb2014cc697a45ecbacf17f0aa919aa6530994452879e277241cf2ee3782f061367244726a6dfec1ffee28a26e8549e153234463297c11a727ba44649";





		// STATIC FIELDS
		/// <summary>
		///     A lock used to initialize and fetch a cached <see cref="DiscoveryDocumentResponse"/>.
		///     See <see cref="_cachedDiscoveryDocumentResponse"/> for more information.
		/// </summary>
		private static object _lockDoscoveryDocumentResponse = new object();
		/// <summary>
		///    A cached instance of a fetched <see cref="DiscoveryDocumentResponse"/>.
		///    The OpenID Connect Discovery Document should be immutable in our Integration Tests, so a cache is kept
		///    and accessed by all running tests.
		/// </summary>
		private static DiscoveryDocumentResponse _cachedDiscoveryDocumentResponse = null;





		// STATIC METHODS
		/// <summary>Retrieves (or builds) the cached Discovery Document for the tests.</summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <returns>Returns a representation of the Discovery Document's response.</returns>
		private static DiscoveryDocumentResponse GetDiscoveryDocumentResponse<TStartup>(WebApplicationFactory<TStartup> webAppFactory)
			where TStartup : class
		{
			lock (_lockDoscoveryDocumentResponse)
			{
				if (_cachedDiscoveryDocumentResponse == null)
				{
					using var httpClient = webAppFactory.CreateClient();
					_cachedDiscoveryDocumentResponse = httpClient.GetDiscoveryDocumentAsync().Result;
				}
			}
			return _cachedDiscoveryDocumentResponse;
		}


		/// <summary>
		///     Parses a received HTTP Redirection response, extracting a specific value from the query parameters of the
		///     target redirection location's URL.
		/// </summary>
		/// <param name="redirectResponse">The HTTP redirection response whose "Location" header will be parsed.</param>
		/// <param name="queryParameterName">The name of the query parameter to be extracted from the "Location" header's URL.</param>
		/// <returns>
		///     <para>In case of success, returns the extracted query parameter's value.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		private static string ReadRedirectLocationQueryParameter(HttpResponseMessage redirectResponse, string queryParameterName)
		{
			string locationQueryString = redirectResponse?.Headers?.Location?.Query;
			if (locationQueryString == null)
				return null;

			var authorizeRequestResponseQueryParams = HttpUtility.ParseQueryString(locationQueryString);
			return authorizeRequestResponseQueryParams[queryParameterName];
		}


		/// <summary>
		///     Parses a received HTTP Redirection response, extracting a specific value from the fragment part of the
		///     target redirection location's URL.
		/// </summary>
		/// <param name="redirectResponse">The HTTP redirection response whose "Location" header will be parsed.</param>
		/// <param name="fragmentParameterName">The name of the fragment parameter to be extracted from the "Location" header's URL.</param>
		/// <returns>
		///     <para>In case of success, returns the extracted fragment parameter's value.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		private static string ReadRedirectLocationFragmentParameter(HttpResponseMessage redirectResponse, string fragmentParameterName)
		{
			string locationFragmentString = redirectResponse?.Headers?.Location?.Fragment?.TrimStart('#');
			if (locationFragmentString == null)
				return null;

			var authorizeRequestResponseQueryParams = HttpUtility.ParseQueryString(locationFragmentString);
			return authorizeRequestResponseQueryParams[fragmentParameterName];
		}


		/// <summary>
		///     <para>Converts a given PKCE Code Verifier value into a PKCE Code Challenge.</para>
		///     <para>
		///         PKCE Code Challenge values can be generated by PKCE-enabled clients by using PKCE Code Verifiers.
		///         Those code Challenges can be sent to the Authorize Endpoint to be associated to Authorize Codes for improved
		///         security against some types of attacks.
		///     </para>
		///     <para>
		///         Then, when a PKCE-enabled client must exchange an Authorization Code for Access/Identity Tokens, the client must
		///         send the PKCE Code Verifier used to generate the previously sent PKCE Code Challenge. Both the Authorization Code
		///         and PKCE Code Verifier will be used to validate that the client requesting the Access/Identity Tokens is the same
		///         client that accessed the Authorize Endpoint previously.
		///     </para>
		/// </summary>
		/// <remarks>
		///     This method follows the implementation defined by the RFC 7636 specification:
		///     <code>code_challenge = BASE64URL-ENCODE(SHA256(ASCII(code_verifier)))</code>
		/// </remarks>
		/// <param name="pkceCodeVerifier">The PKCE Code Verifier from which a PKCE Code Challenge will be generated.</param>
		/// <returns>Returns a string representation of the PKCE Code Challenge generated for the given PKCE Code Challenge.</returns>
		public static string TransformPkceCodeVerifierIntoCodeChallenge(string pkceCodeVerifier)
			=> WebEncoders.Base64UrlEncode(Encoding.ASCII.GetBytes(FakePkceCodeVerifier).Sha256());


		/// <summary>Performs a call to the IdentityServer4's Authorize Endpoint.</summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <param name="authorizeEndpointQueryParams">
		///     The parameters that should be passed in the query string part of the URL that will be used to call
		///     the Authorize Endpoint.
		/// </param>
		/// <param name="httpClient">
		///     <para>
		///         If this reference is specified, the HttpClient can have it's internal cookies modified by the
		///         authentication procedure.
		///     </para>
		///     <para>
		///         This instance is expected to be configured to NOT automatically follow HTTP redirections,
		///         as these will be treated internally.
		///     </para>

		/// </param>
		/// <typeparam name="TStartup">The type of the startup class used by the test server.</typeparam>
		/// <returns>Returns a string containing the URL where the user needs to be redirected to after a successful login is performed.</returns>
		/// <exception cref="ArgumentNullException">
		///     Thrown if any of this method's parameters receive a <c>null</c> value.
		///     All of the arguments must be provided with valid values.
		/// </exception>
		/// <exception cref="AuthorizeEndpointResponseException">Thrown if an invalid/erroneous response has been returned by the Authorization Endpoint.</exception>
		/// <exception cref="DiscoveryDocumentRetrieveErrorException">Thrown if there is any kind of error while trying to retrieve the IdP Discovery Document.</exception>
		private static async Task<string> CallAuthorizeEndpointAsync<TStartup>(
			WebApplicationFactory<TStartup> webAppFactory,
			IEnumerable<KeyValuePair<string, string>> authorizeEndpointQueryParams,
			HttpClient httpClient)
			where TStartup : class
		{
			// Error checks
			if (webAppFactory == null)
				throw new ArgumentNullException(nameof(webAppFactory));
			if (authorizeEndpointQueryParams == null)
				throw new ArgumentNullException(nameof(authorizeEndpointQueryParams));
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));


			// Retrieve the discovery document from the proper IdP endpoint
			var oidcDiscoveryDoc = GetDiscoveryDocumentResponse(webAppFactory);
			if (oidcDiscoveryDoc.IsError)
				throw new DiscoveryDocumentRetrieveErrorException("Failed to retrieve information from the IdP Discovery Endpoint.") { DiscoveryDocumentResponse = oidcDiscoveryDoc };

			// Perform a request to the Authorization Endpoint with a non-redirecting HTTP Client.
			// This is expected to return an HTTP redirection which would normally redirect the unauthenticated user to a login page.
			var queryBuilder = new QueryBuilder(authorizeEndpointQueryParams);
			var authorizeEndpointUri = new Uri(oidcDiscoveryDoc.AuthorizeEndpoint);
			var uriToCall = $"{authorizeEndpointUri.AbsolutePath}{queryBuilder.ToQueryString()}";

			var authorizeRequestResponseMessage = await httpClient.GetAsync(uriToCall);

			// The "Location" HTTP Header in the response will contain the target URL for the redirection. From this target URL,
			// we must extract the "return URL" query parameter, which must be passed along with the user's credentials during login
			// in order for the IdentityServer4 to be able to correctly perform the user's authentication
			string returnUrlAfterLogin = ReadRedirectLocationQueryParameter(authorizeRequestResponseMessage, nameof(LoginInputModel.ReturnUrl));
			if (returnUrlAfterLogin == null)
			{
				throw new AuthorizeEndpointResponseException($@"Failed to extract ""{nameof(LoginInputModel.ReturnUrl)}"" from the Authorization Endpoint's response (""{nameof(authorizeRequestResponseMessage.Headers.Location)}"" HTTP header).")
				{
					Response = authorizeRequestResponseMessage,
					RequestUri = uriToCall,
				};
			}

			// Returns the URL to which the user needs to be redirected after a successful login is performed
			return returnUrlAfterLogin;
		}


		/// <summary>
		///     Utility method for trying to perform a login operation with the given user's account.
		///     The Authorize Endpoint running in the Test Host will be called as appropriate.
		/// </summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <param name="targetUser">The user for whom you are trying to perform a login operation.</param>
		/// <param name="authorizeEndpointQueryParams">
		///     The parameters that should be passed in the query string part of the URL that will be used to call
		///     the Authorize Endpoint.
		/// </param>
		/// <param name="httpClient">
		///     <para>Optional reference to an <see cref="HttpClient"/> which will be used during the login operation.</para>
		///     <para>
		///         If this reference is specified, the HttpClient can have it's internal cookies modified by the
		///         authentication procedure. Also, this instance is expected to be configured to NOT automatically follow HTTP redirections,
		///         as these will be treated internally.
		///     </para>
		///     <para>If this reference is not provided, the <paramref name="webAppFactory"/> argument will be used to generate HTTP clients as necessary.</para>
		/// </param>
		/// <typeparam name="TStartup">The type of the startup class used by the test server.</typeparam>
		/// <returns>
		///     <para>In case of success, returns a <see cref="LoginOutputModel"/> containing the logged-in user's data.</para>
		///     <para>
		///         If the login endpoint was successfully reached but it returned an error (e.g., due to invalid user credentials, validation failure on
		///         the <see cref="LoginInputModel"/> object, etc), this method returns <c>null</c>.
		///     </para>
		///     <para>For any other conditions (e.g., invalid client ID, malformed requests, etc), an <see cref="AuthorizeEndpointResponseException"/> will be thrown.</para>
		/// </returns>
		/// <exception cref="DiscoveryDocumentRetrieveErrorException">Thrown if there is any kind of error while trying to retrieve the IdP Discovery Document.</exception>
		/// <exception cref="AuthorizeEndpointResponseException">Thrown if an invalid/erroneous response has been returned by the Authorization Endpoint.</exception>
		public static async Task<LoginOutputModel> PerformUserLoginAsync<TStartup>(
			WebApplicationFactory<TStartup> webAppFactory,
			TestUser targetUser,
			IEnumerable<KeyValuePair<string, string>> authorizeEndpointQueryParams,
			HttpClient httpClient = null)
			where TStartup : class
		{
			// Call the Authorize Endpoint to retrieve a post-login return URL
			httpClient = httpClient ?? webAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
			string returnUrlAfterLogin = await CallAuthorizeEndpointAsync(webAppFactory, authorizeEndpointQueryParams, httpClient);

			// Send the user credentials and post-login return URL to the login endpoint
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;
			var loginInputData = new LoginInputModel
			{
				Email = targetUserEmail,
				Password = targetUser.Password,
				ReturnUrl = returnUrlAfterLogin,
			};
			var loginResult = await httpClient.PostAsync("/api/Account/login", JsonContent.Create(loginInputData));

			// Return the result to the client code as appropriate
			if (loginResult.IsSuccessStatusCode)
				return await loginResult.Content.ReadFromJsonAsync<LoginOutputModel>();
			return null;
		}


		/// <summary>Performs the Authorization Code Flow and retrieves a token for a user.</summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <param name="targetUser">The user for whom you are trying to retrieve a token.</param>
		/// <param name="targetClient">The OIDC/OAuth Client for which the token is being retrieved.</param>
		/// <param name="clientSecret">Secret for the OIDC/OAuth Client (if needed).</param>
		/// <param name="httpClient">
		///     <para>Optional reference to an <see cref="HttpClient"/> which will be used during the login operation.</para>
		///     <para>
		///         If this reference is specified, the HttpClient can have it's internal cookies modified by the
		///         authentication procedure. Also, this instance is expected to be configured to NOT automatically follow HTTP redirections,
		///         as these will be treated internally.
		///     </para>
		///     <para>If this reference is not provided, the <paramref name="webAppFactory"/> argument will be used to generate HTTP clients as necessary.</para>
		/// </param>
		/// <typeparam name="TStartup">The type of the startup class used by the test server.</typeparam>
		/// <returns>Returns an object containing information about the token retrieval response.</returns>
		public static async Task<TokenResponse> RetrieveUserTokenForAuthorizationCodeFlowAsync<TStartup>(
			WebApplicationFactory<TStartup> webAppFactory,
			TestUser targetUser,
			Client targetClient,
			string clientSecret,
			HttpClient httpClient = null)
			where TStartup : class
		{
			// Prepare the data
			httpClient = httpClient ?? webAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

			var returnUrlAfterLogin = targetClient.RedirectUris.First();
			var authorizeEndpointQueryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
			};
			if (targetClient.RequirePkce)
			{
				// A Code Challenge must be sent for clients that require PKCE verification
				string codeChallenge = TransformPkceCodeVerifierIntoCodeChallenge(FakePkceCodeVerifier);
				authorizeEndpointQueryParams.Add(OidcConstants.AuthorizeRequest.CodeChallenge, codeChallenge);
				authorizeEndpointQueryParams.Add(OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256);
			}



			// Log the user in and extract the authorization code which would be returned through an HTTP client redirection
			var loggedInUser = await PerformUserLoginAsync(webAppFactory, targetUser, authorizeEndpointQueryParams, httpClient);

			var returnToClientResponse = await httpClient.GetAsync(loggedInUser.ReturnUrl);
			string authorizationCode = ReadRedirectLocationQueryParameter(returnToClientResponse, OidcConstants.AuthorizeResponse.Code);


			// Request the token
			var discoveryDocument = GetDiscoveryDocumentResponse(webAppFactory);
			var tokenRequestData = new AuthorizationCodeTokenRequest
			{
				Address = discoveryDocument.TokenEndpoint,

				ClientId = targetClient.ClientId,
				ClientSecret = clientSecret,

				Code = authorizationCode,
				RedirectUri = returnUrlAfterLogin,
			};
			if (targetClient.RequirePkce)
				tokenRequestData.CodeVerifier = FakePkceCodeVerifier;

			var tokenResult = await httpClient.RequestAuthorizationCodeTokenAsync(tokenRequestData);
			return tokenResult;
		}


		/// <summary>Performs the Implicit Flow and retrieves a token for a user.</summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <param name="targetUser">The user for whom you are trying to retrieve a token.</param>
		/// <param name="targetClient">The OIDC/OAuth Client for which the token is being retrieved.</param>
		/// <param name="httpClient">
		///     <para>Optional reference to an <see cref="HttpClient"/> which will be used during the login operation.</para>
		///     <para>
		///         If this reference is specified, the HttpClient can have it's internal cookies modified by the
		///         authentication procedure. Also, this instance is expected to be configured to NOT automatically follow HTTP redirections,
		///         as these will be treated internally.
		///     </para>
		///     <para>If this reference is not provided, the <paramref name="webAppFactory"/> argument will be used to generate HTTP clients as necessary.</para>
		/// </param>
		/// <typeparam name="TStartup">The type of the startup class used by the test server.</typeparam>
		/// <returns>
		///     <para>In case of success, returns a string containing the obtained token.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		public static async Task<string> RetrieveUserTokenForImplicitFlowAsync<TStartup>(
			WebApplicationFactory<TStartup> webAppFactory,
			TestUser targetUser,
			Client targetClient,
			HttpClient httpClient = null)
			where TStartup : class
		{
			// Prepare the data
			httpClient = httpClient ?? webAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

			var returnUrlAfterLogin = targetClient.RedirectUris.First();
			var authorizeEndpointQueryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token },
			};



			// Log the user in and extract the authorization code which would be returned through an HTTP client redirection
			var loggedInUser = await PerformUserLoginAsync(webAppFactory, targetUser, authorizeEndpointQueryParams, httpClient);

			var returnToClientResponse = await httpClient.GetAsync(loggedInUser.ReturnUrl);
			string accessToken = ReadRedirectLocationFragmentParameter(returnToClientResponse, OidcConstants.AuthorizeResponse.AccessToken);

			return accessToken;
		}


		/// <summary>Performs the Client Credentials Flow and retrieves a token for a client.</summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <param name="targetClient">The OIDC/OAuth Client for which the token is being retrieved.</param>
		/// <param name="clientSecret">Secret for the OIDC/OAuth Client.</param>
		/// <param name="httpClient">
		///     <para>Optional reference to an <see cref="HttpClient"/> which will be used during the login operation.</para>
		///     <para>
		///         If this reference is specified, the HttpClient can have it's internal cookies modified by the
		///         authentication procedure. Also, this instance is expected to be configured to NOT automatically follow HTTP redirections,
		///         as these will be treated internally.
		///     </para>
		///     <para>If this reference is not provided, the <paramref name="webAppFactory"/> argument will be used to generate HTTP clients as necessary.</para>
		/// </param>
		/// <typeparam name="TStartup">The type of the startup class used by the test server.</typeparam>
		/// <returns>
		///     <para>In case of success, returns a string containing the obtained token.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		public static async Task<TokenResponse> RetrieveTokenForClientCredentialsFlowAsync<TStartup>(
			WebApplicationFactory<TStartup> webAppFactory,
			Client targetClient,
			string clientSecret,
			HttpClient httpClient = null)
			where TStartup : class
		{
			// Prepare the data
			httpClient = httpClient ?? webAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
			string tokenScopes = string.Join(" ", targetClient.AllowedScopes);

			// Request the token
			var discoveryDocument = GetDiscoveryDocumentResponse(webAppFactory);
			var tokenResult = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = discoveryDocument.TokenEndpoint,

				ClientId = targetClient.ClientId,
				ClientSecret = clientSecret,
				Scope = tokenScopes,
			});

			return tokenResult;
		}


		/// <summary>Performs the Resource Owner Password Flow and retrieves a token for a user.</summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <param name="targetClient">The OIDC/OAuth Client for which the token is being retrieved.</param>
		/// <param name="clientSecret">Secret for the OIDC/OAuth Client.</param>
		/// <param name="userName">The user name for the user whose token is being retrieved.</param>
		/// <param name="userPassword">The user password for the user whose token is being retrieved.</param>
		/// <param name="httpClient">
		///     <para>Optional reference to an <see cref="HttpClient"/> which will be used during the login operation.</para>
		///     <para>
		///         If this reference is specified, the HttpClient can have it's internal cookies modified by the
		///         authentication procedure. Also, this instance is expected to be configured to NOT automatically follow HTTP redirections,
		///         as these will be treated internally.
		///     </para>
		///     <para>If this reference is not provided, the <paramref name="webAppFactory"/> argument will be used to generate HTTP clients as necessary.</para>
		/// </param>
		/// <typeparam name="TStartup">The type of the startup class used by the test server.</typeparam>
		/// <returns>
		///     <para>In case of success, returns a string containing the obtained token.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		public static async Task<TokenResponse> RetrieveTokenForResourceOwnerPasswordFlowAsync<TStartup>(
			WebApplicationFactory<TStartup> webAppFactory,
			Client targetClient,
			string clientSecret,
			string userName,
			string userPassword,
			HttpClient httpClient = null)
			where TStartup : class
		{
			// Prepare the data
			httpClient = httpClient ?? webAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
			string tokenScopes = string.Join(" ", targetClient.AllowedScopes);

			// Request the token
			var discoveryDocument = GetDiscoveryDocumentResponse(webAppFactory);
			var tokenResult = await httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
			{
				Address = discoveryDocument.TokenEndpoint,

				UserName = userName,
				Password = userPassword,

				ClientId = targetClient.ClientId,
				ClientSecret = clientSecret,
				Scope = tokenScopes,
			});

			return tokenResult;
		}
	}
}