using IdentityModel;
using IdentityModel.Client;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Models;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Exceptions;
using SimpleOidcOauth.Tests.Integration.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Tests.Integration.Utilities
{
	/// <summary>Utility class with methods to make it easier to deal with user authentication matters during the tests.</summary>
	static class AuthenticationUtilities
	{
		// CONSTANTS
		/// <summary>A fake PKCE Code Verifier to be used during tests, whenever necessary.</summary>
		private const string FakePkceCodeVerifier = "ac325f2bb2014cc697a45ecbacf17f0aa919aa6530994452879e277241cf2ee3782f061367244726a6dfec1ffee28a26e8549e153234463297c11a727ba44649";
		/// <summary>
		///     <para>A fake "nonce" value to be used in tests.</para>
		///     <para>A "nonce" value is required to be sent to the Authorization Endpoint for both the Implicit Flow and the Hybrid Flow.</para>
		/// </summary>
		private const string FakeNonceValue = "6ebf5d23db884e39b2871f5b88fd3155020a1956ceaa418eadc8cf2276817ea58cab272c84c247c9b99a748273fff757513b4e899c804cfc8eaf9d26a3938821";





		// STATIC METHODS
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
			var oidcDiscoveryDoc = DiscoveryDocumentUtilities.GetDiscoveryDocumentResponse(webAppFactory);
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
			string returnUrlAfterLogin = WebUtilities.ReadRedirectLocationQueryParameter(authorizeRequestResponseMessage, nameof(LoginInputModel.ReturnUrl));
			if (returnUrlAfterLogin == null)
			{
				// If the returned answer was a redirection, it was probably a redirection to the IdP Error Endpoint.
				// Let's use that information to generate an exception.
				if (authorizeRequestResponseMessage.StatusCode == HttpStatusCode.Found)
				{
					var errorRedirectionResponse = await httpClient.ManuallyFollowRedirectResponse(authorizeRequestResponseMessage);
					var errorRedirectionResponseStr = await errorRedirectionResponse.Content.ReadAsStringAsync();
					throw new AuthorizeEndpointResponseException($@"Failed to extract ""{nameof(LoginInputModel.ReturnUrl)}"" from the Authorization Endpoint's response (""{nameof(authorizeRequestResponseMessage.Headers.Location)}"" HTTP header).")
					{
						AuthorizeEndpointResponse = authorizeRequestResponseMessage,
						ErrorEndpointResponse = errorRedirectionResponse,
						ErrorEndpointResponseString = errorRedirectionResponseStr,
						RequestUri = uriToCall,
					};
				}

				throw new AuthorizeEndpointResponseException($@"Failed to extract ""{nameof(LoginInputModel.ReturnUrl)}"" from the Authorization Endpoint's response (""{nameof(authorizeRequestResponseMessage.Headers.Location)}"" HTTP header).")
				{
					AuthorizeEndpointResponse = authorizeRequestResponseMessage,
					RequestUri = uriToCall,
				};
			}

			// Returns the URL to which the user needs to be redirected after a successful login is performed
			return returnUrlAfterLogin;
		}


		/// <summary>Performs an HTTP Request to the login endpoint.</summary>
		/// <remarks>
		///     This method uses the given HTTP Client to perform a login request, sending user credentials in the request's body.
		///     The used <see cref="HttpClient"/> will receive and store authentication cookies to use for further authenticated requests (provided
		///     that the <see cref="HttpClient"/> is not configured to discard/ignore these cookies in any ways, of course).
		/// </remarks>
		/// <param name="httpClient">An HTTP Client used to communicate with the Test Host.</param>
		/// <param name="userName">The user name, to be sent as the user's credentials.</param>
		/// <param name="password">The password of the user, to be sent as the user's credentials.</param>
		/// <param name="returnUrl">
		///     <para>The Return URL, as returned by a previous call to the OAuth 2.0 Authorize Endpoint.</para>
		///     <para>
		///         This parameter is only necessary for OAuth authorization flows. It might be set to <c>null</c> for cases
		///         where the only need is to perform a login operation to obtain authentication cookies (e.g., for calling the IdP Management Interface APIs to add a
		///         new Client Application, User, API Resource, etc).
		///     </para>
		/// </param>
		/// <returns>
		///     Returns an object containing the HTTP Response message.
		///     The possible contents of this response are described in the documentation for the POST method on the <see cref="AppEndpoints.LoginUri"/> endpoint.
		/// </returns>
		public static async Task<HttpResponseMessage> PerformRequestToLoginEndpointAsync(HttpClient httpClient, string userName, string password, string returnUrl = null)
		{
			var loginInputData = new LoginInputModel
			{
				Email = userName,
				Password = password,
				ReturnUrl = returnUrl ?? string.Empty,
			};
			var loginResult = await httpClient.PostAsync(AppEndpoints.LoginUri, JsonContent.Create(loginInputData));
			return loginResult;
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
			httpClient = httpClient ?? webAppFactory.CreateIntegrationTestClient(false);;
			string returnUrlAfterLogin = await CallAuthorizeEndpointAsync(webAppFactory, authorizeEndpointQueryParams, httpClient);

			// Send the user credentials and post-login return URL to the login endpoint
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;
			var loginResult = await PerformRequestToLoginEndpointAsync(httpClient, targetUserEmail, targetUser.Password, returnUrlAfterLogin);

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
			httpClient = httpClient ?? webAppFactory.CreateIntegrationTestClient(false);

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
			string authorizationCode = WebUtilities.ReadRedirectLocationQueryParameter(returnToClientResponse, OidcConstants.AuthorizeResponse.Code);


			// Request the token
			var discoveryDocument = DiscoveryDocumentUtilities.GetDiscoveryDocumentResponse(webAppFactory);
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
			httpClient = httpClient ?? webAppFactory.CreateIntegrationTestClient(false);

			var returnUrlAfterLogin = targetClient.RedirectUris.First();
			var authorizeEndpointQueryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.Nonce, FakeNonceValue },
			};

			if (targetClient.ClientId == TestData.ClientImplicitFlowAccessTokensOnly.ClientId)
				authorizeEndpointQueryParams.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token);
			else if (targetClient.ClientId == TestData.ClientImplicitFlowAccessAndIdTokens.ClientId)
				authorizeEndpointQueryParams.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken);
			else
				throw new NotImplementedException($@"Failed to retrieve Implicit Flow token: specific configurations not implemented for client with ID ""{targetClient.ClientId}"".");



			// Log the user in and extract the authorization code which would be returned through an HTTP client redirection
			var loggedInUser = await PerformUserLoginAsync(webAppFactory, targetUser, authorizeEndpointQueryParams, httpClient);

			var returnToClientResponse = await httpClient.GetAsync(loggedInUser.ReturnUrl);
			string accessToken = WebUtilities.ReadRedirectLocationFragmentParameter(returnToClientResponse, OidcConstants.AuthorizeResponse.AccessToken);

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
			httpClient = httpClient ?? webAppFactory.CreateIntegrationTestClient(false);
			string tokenScopes = string.Join(" ", targetClient.AllowedScopes);

			// Request the token
			var discoveryDocument = DiscoveryDocumentUtilities.GetDiscoveryDocumentResponse(webAppFactory);
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
			httpClient = httpClient ?? webAppFactory.CreateIntegrationTestClient(false);
			string tokenScopes = string.Join(" ", targetClient.AllowedScopes);

			// Request the token
			var discoveryDocument = DiscoveryDocumentUtilities.GetDiscoveryDocumentResponse(webAppFactory);
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