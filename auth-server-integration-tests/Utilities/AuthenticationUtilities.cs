using IdentityModel;
using IdentityModel.Client;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Models;
using SimpleOidcOauth.Tests.Integration.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace SimpleOidcOauth.Tests.Integration.Utilities
{
	/// <summary>Utility class with methods make it easier to deal with user authentication matters during the tests.</summary>
	static class AuthenticationUtilities
	{
		/// <summary>
		///     Utility method for trying to perform a login operation with the given user's account.
		///     The OAuth/OIDC endpoints running in the Test Host will be contacted, and the <see cref="AccountController.Login(LoginInputModel)"/> endpoint
		///     will be called as appropriate.
		/// </summary>
		/// <param name="targetUser">The user for whom you are trying to perform a login operation.</param>
		/// <param name="authorizeEndpointQueryParams">
		///     The parameters that should be passed in the query string part of the URL that will be used to call
		///     the Authorize Endpoint.
		/// </param>
		/// <param name="httpClient">
		///     <para>Optional reference to an <see cref="HttpClient"/> which will be used during the login operation.</para>
		///     <para>
		///         If this reference is specified, the HttpClient can have it's internal cookies modified by the
		///         authentication procedure. Also, this instance is expected to be configured for automatically following HTTP redirections,
		///         which are important for the OAuth/OpenID Connect flows.
		///     </para>
		/// </param>
		/// <returns>
		///     <para>In case of success, returns a <see cref="LoginOutputModel"/> containing the logged-in user's data.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		/// <exception cref="DiscoveryDocumentRetrieveErrorException">Thrown if there is any kind of error while trying to retrieve the IdP Discovery Document.</exception>
		/// <exception cref="AuthorizeEndpointResponseException">Thrown if an invalid/erroneous response has been returned by the Authorization Endpoint.</exception>
		public static async Task<LoginOutputModel> PerformUserLoginAsync<TStartup>(
			WebApplicationFactory<TStartup> _webAppFactory, TestUser targetUser,
			Dictionary<string, string> authorizeEndpointQueryParams, HttpClient httpClient = null)
			where TStartup : class
		{
			httpClient = httpClient ?? _webAppFactory.CreateClient();
			HttpClient httpNonFollowingClient = _webAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });


			// Retrieve the discovery document from the proper IdP endpoint
			var oidcDiscoveryDoc = await httpClient.GetDiscoveryDocumentAsync();
			if (oidcDiscoveryDoc.IsError)
				throw new DiscoveryDocumentRetrieveErrorException("Failed to retrieve information from the IdP Discovery Endpoint.") { DiscoveryDocumentResponse = oidcDiscoveryDoc };

			// Perform a request to the Authorization Endpoint with the non-redirecting HTTP Client.
			// This is expected to return an HTTP redirection which would normally redirect the unauthenticated user to a login page.
			var queryBuilder = new QueryBuilder(authorizeEndpointQueryParams);
			var authorizeEndpointUri = new Uri(oidcDiscoveryDoc.AuthorizeEndpoint);
			var uriToCall = $"{authorizeEndpointUri.AbsolutePath}{queryBuilder.ToQueryString()}";

			var authorizeRequestResponseMessage = await httpNonFollowingClient.GetAsync(uriToCall);

			// The "Location" HTTP Header in the response will contain the target URL for the redirection. From this target URL,
			// we must extract the "return URL" query parameter, which must be passed along with the user's credentials during login
			// in order for the IdentityServer4 to be able to correctly perform the user's authentication
			var authorizeRequestResponseQueryParams = HttpUtility.ParseQueryString(authorizeRequestResponseMessage.Headers.Location.Query);
			var returnUrlAfterLogin = authorizeRequestResponseQueryParams[nameof(LoginInputModel.ReturnUrl)];
			if (returnUrlAfterLogin == null)
			{
				throw new AuthorizeEndpointResponseException($@"Failed to extract ""{nameof(LoginInputModel.ReturnUrl)}"" from the Authorization Endpoint's response (""{nameof(authorizeRequestResponseMessage.Headers.Location)}"" HTTP header).")
				{
					Response = authorizeRequestResponseMessage,
					RequestUri = uriToCall,
				};
			}

			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;
			var loginInputData = new LoginInputModel
			{
				Email = targetUserEmail,
				Password = targetUser.Password,
				ReturnUrl = returnUrlAfterLogin,
			};
			var loginResult = await httpClient.PostAsync("/api/Account/login", JsonContent.Create(loginInputData));

			if (loginResult.IsSuccessStatusCode)
				return await loginResult.Content.ReadFromJsonAsync<LoginOutputModel>();
			return null;
		}
	}
}