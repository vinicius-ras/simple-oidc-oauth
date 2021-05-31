using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleOidcOauth.Data.Configuration;

namespace SimpleOidcOauth.IdentityServer
{
	/// <summary>
	///     <para>A customized implementation of IdentityServer's <see cref="IReturnUrlParser"/>.</para>
	///     <para>
	///         IdentityServer4's default <see cref="IReturnUrlParser"/> implementation only allows parsing of local URLs.
	///         This limits the IdP UI to basically be served only from the same server as the IdP, restricting the usage of a separate server
	///         which could serve a SPA-based UI.
	///     </para>
	///     <para>
	///         The <see cref="CustomReturnUrlParser"/> implementation allows the parsing of local URLs like the default implementation, while it also allows
	///         the parsing of URLs from a well-known SPA server. The SPA server's origin (scheme, host, and port) can be configured using ASP.NET Core's configuration
	///         infrastructure.
	///     </para>
	///
	///     <para>The SPA's base URL can be set at the <see cref="SpaConfigs.BaseUrl"/> configuration.</para>
	/// </summary>
	class CustomReturnUrlParser : IReturnUrlParser
	{
		// CONSTANTS
		/// <summary>
		///     Collection of prefixes to be considered required for Local URLs.
		///     A URL can only be considered a Local URL if it starts with one of these prefixes.
		/// </summary>
		/// <value>
		///     An enumerable collection of strings which are to be considered mandatory for Local URLs.
		///     The URL must match at least one of these prefixes to be considered a possible "Local URL".
		/// </value>
		private static IEnumerable<string> _possibleLocalUrlPrefixes = new [] {"/", "~/"};
		/// <summary>Collection of prefixes to be considered invalid for Local URLs.</summary>
		/// <value>An enumerable collection of strings which are to be considered invalid prefixes for Local URLs.</value>
		private static IEnumerable<string> _invalidLocalUrlPrefixes = new [] {"//", @"/\", "~//", @"~/\"};





		// PRIVATE FIELDS
		/// <summary>Container-injected instance for the <see cref="IAuthorizeRequestValidator" /> service.</summary>
		private readonly IAuthorizeRequestValidator _validator;
		/// <summary>Container-injected instance for the <see cref="IUserSession" /> service.</summary>
		private readonly IUserSession _userSession;
		/// <summary>Container-injected instance for the <see cref="ILogger" /> service.</summary>
		private readonly ILogger _logger;
		/// <summary>Container-injected instance for the <see cref="IOptions{AppConfigs}" /> service.</summary>
		private readonly IOptions<AppConfigs> _appConfigs;
		/// <summary>Container-injected instance for the <see cref="IAuthorizationParametersMessageStore" /> service.</summary>
		private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;





		// PRIVATE STATIC METHODS
		/// <summary>Checks if a given URL can be considered a "Local URL".</summary>
		/// <param name="url">The URL that needs to be checked.</param>
		/// <returns>Returns a flag indicating if the URL is to be considered a "Local URL".</returns>
		private static bool CheckIsLocalUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				return false;

			bool urlStartsWithLocalPrefix = _possibleLocalUrlPrefixes.Any(prefix => url.StartsWith(prefix)),
				urlHasInvalidPrefix = _invalidLocalUrlPrefixes.Any(prefix => url.StartsWith(prefix));
			return (urlStartsWithLocalPrefix && urlHasInvalidPrefix == false);
		}





		// PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="validator">Container-injected instance for the <see cref="IAuthorizeRequestValidator" /> service.</param>
		/// <param name="userSession">Container-injected instance for the <see cref="IUserSession" /> service.</param>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{CustomReturnUrlParser}" /> service.</param>
		/// <param name="appConfigs">Container-injected instance for the <see cref="IOptions{TOptions}" /> service.</param>
		/// <param name="authorizationParametersMessageStore">Container-injected instance for the <see cref="IAuthorizationParametersMessageStore" /> service.</param>
		public CustomReturnUrlParser(
			IAuthorizeRequestValidator validator,
			IUserSession userSession,
			ILogger<CustomReturnUrlParser> logger,
			IOptions<AppConfigs> appConfigs,
			IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
		{
			_validator = validator;
			_userSession = userSession;
			_logger = logger;
			_appConfigs = appConfigs;
			_authorizationParametersMessageStore = authorizationParametersMessageStore;
		}





		// INTERFACE IMPLEMENTATION: IReturnUrlParser
		/// <summary>Tries to perform the parsing of a return URL.</summary>
		/// <param name="returnUrl">The return URL that needs to be parsed.</param>
		/// <returns>
		///     <para>In case of success, returns a <see cref="AuthorizationRequest"/> object representing the parsed URL.</para>
		///     <para>In case of failure, returns a <c>null</c> value.</para>
		/// </returns>
		public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
		{
			// First we must verify if the return URL is valid
			if (IsValidReturnUrl(returnUrl) == false)
				return null;


			// Retrieve the URL's Query String, and try to parse it into a NameValueCollection
			var queryStringNameValueCollection = new NameValueCollection();
			if (string.IsNullOrEmpty(returnUrl) == false)
			{
				string queryToParse = null;
				if (CheckIsLocalUrl(returnUrl))
				{
					// Find the Local URL's query string
					int queryStartIndex = queryToParse.IndexOf("?");
					if (queryStartIndex >= 0)
						queryToParse = returnUrl.Substring(queryStartIndex);
				}
				else
				{
					// Parse external URL
					try
					{
						var parsedReturnUri = new Uri(returnUrl);
						queryToParse = parsedReturnUri.Query;
					}
					catch (UriFormatException ex)
					{
						// Failed to parse the URL
						_logger.LogDebug(ex, $"Failed to parse return URL due to {nameof(UriFormatException)}: {returnUrl}");
						return null;
					}
					catch (InvalidOperationException ex)
					{
						// Failed to parse the URL's query string
						_logger.LogDebug(ex, $"Failed to parse return URL's Query String: {returnUrl}");
						return null;
					}
				}

				var parsedQueryDictionary = QueryHelpers.ParseQuery(queryToParse ?? "?");
				foreach (var parsedItem in parsedQueryDictionary)
					queryStringNameValueCollection.Add(parsedItem.Key, parsedItem.Value.First());
			}


			// Retrieve the authorization parameters from the message store, if applicable
			if (_authorizationParametersMessageStore != null)
			{
				// Retrieve parameters from message store
				var messageStoreId = queryStringNameValueCollection[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
				var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);

				// Replace our current value collection with the incoming message store's data
				queryStringNameValueCollection.Clear();
				if (entry != null)
				{
					foreach (var collectionEntry in entry.Data)
					{
						string collectionEntryKey = collectionEntry.Key;
						foreach (var collectionElement in collectionEntry.Value)
							queryStringNameValueCollection.Add(collectionEntryKey, collectionElement);
					}
				}
			}


			// Try to validate the authorization request by using IdentityServer's services
			var user = await _userSession.GetUserAsync();
			var validationResult = await _validator.ValidateAsync(queryStringNameValueCollection, user);
			if (validationResult.IsError)
				return null;


			// Create the resulting AuthorizationRequest.
			// This code mirrors the behavior of the AuthorizationRequest's constructor method which receives
			// a ValidatedAuthorizeRequest and builds a new AuthorizationRequest object from it.
			// That constructor is present at IdentityServer 4.0.4, and is not accessible outside of the IdentityServer's
			// assemblies, due to its "internal" modifier.
			var validatedRequest = validationResult.ValidatedRequest;
			var resultAuthRequest = new AuthorizationRequest
			{
				Client = validatedRequest.Client,
				RedirectUri = validatedRequest.RedirectUri,
				DisplayMode = validatedRequest.DisplayMode,
				UiLocales = validatedRequest.UiLocales,
				IdP = validatedRequest.GetIdP(),
				Tenant = validatedRequest.GetTenant(),
				LoginHint = validatedRequest.LoginHint,
				PromptModes = validatedRequest.PromptModes,
				AcrValues = validatedRequest.GetAcrValues(),
				ValidatedResources = validatedRequest.ValidatedResources,
			};

			resultAuthRequest.Parameters.Add(validatedRequest.Raw);
			foreach (var entry in validatedRequest.RequestObjectValues)
				resultAuthRequest.RequestObjectValues.Add(entry.Key, entry.Value);

			return resultAuthRequest;
		}


		/// <summary>
		///     Verifies if the given return URL is valid.
		///     A valid return URL needs to either be a Local URL, or a well-known External/Remote URL configured
		///     as the auth-server's origin (see <see cref="AuthServerConfigs.BaseUrl"/>).
		/// </summary>
		/// <param name="returnUrl">The return URL to be verified.</param>
		/// <returns>Returns a flag indicating if the specified return URL is valid.</returns>
		public bool IsValidReturnUrl(string returnUrl)
		{
			// To be considered valid, the return URL must be either local or have the
			// same origin as configured for this authorization server
			bool isLocalUrl = CheckIsLocalUrl(returnUrl),
				isAuthServerUrl = false;

			if (isLocalUrl == false)
			{
				Uri returnUrlUri = null,
					authServerUri = null;

				try
				{
					returnUrlUri = new Uri(returnUrl);
					authServerUri = new Uri(_appConfigs.Value.AuthServer.BaseUrl);
				}
				catch (UriFormatException ex)
				{
					_logger.LogDebug(ex, $"Failed to validate a return URL: an invalid URL was provided as a return URL and/or as the auth-server's URL.");
					return false;
				}

				int uriCompareResult = Uri.Compare(
					returnUrlUri,
					authServerUri,
					UriComponents.SchemeAndServer,
					UriFormat.Unescaped,
					StringComparison.CurrentCultureIgnoreCase
				);

				isAuthServerUrl = (uriCompareResult == 0);
			}

			if (isLocalUrl == false && isAuthServerUrl == false)
			{
				_logger.LogTrace($"Invalid return URL: {returnUrl}");
				return false;
			}

			string returnUrlWithoutQueryString = returnUrl;
			var index = returnUrl.IndexOf('?');
			if (index >= 0)
				returnUrlWithoutQueryString = returnUrl.Substring(0, index);

			if (returnUrlWithoutQueryString.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
				returnUrlWithoutQueryString.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
			{
				_logger.LogTrace($"Invalid return URL ending: {returnUrl}");
				return true;
			}

			return false;
		}





		/// <summary>
		///     Constants extracted from IdentityServer4's code base, which represent parammeter names and
		///     endpoint paths used internally by IdentityServer.
		/// </summary>
		private static class Constants
		{
			/// <summary>
			///     Constants representing the name of authorization parameters received
			///     by IdentityServer4's Authorization Endpoint.
			/// </summary>
			public static class AuthorizationParamsStore
			{
				/// <summary>
				///     The name of a parameter received by IdentityServer4's Authorization Endpoint which
				///     informs a Message Store ID value.
				/// </summary>
				public const string MessageStoreIdParameterName = "authzId";
			}





			/// <summary>
			///     Constants representing endpoint paths used by IdentityServer4's Authorization Endpoint.
			/// </summary>
			public static class ProtocolRoutePaths
			{
				/// <summary>Represents the Authorize Endpoint's path.</summary>
				public const string Authorize = "connect/authorize";
				/// <summary>
				///     Represents the Authorize Endpoint's Callback path.
				///     This path will receive authorization's context data that is be used to
				///     process Authorization Requests.
				/// </summary>
				public const string AuthorizeCallback = Authorize + "/callback";
			}
		}
	}
}