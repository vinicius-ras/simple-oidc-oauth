using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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
		public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
		{
			if (IsValidReturnUrl(returnUrl))
			{
				var parameters = returnUrl.ReadQueryStringAsNameValueCollection();
				if (_authorizationParametersMessageStore != null)
				{
					var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
					var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
					parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();
				}

				var user = await _userSession.GetUserAsync();
				var result = await _validator.ValidateAsync(parameters, user);
				if (!result.IsError)
					return result.ValidatedRequest.ToAuthorizationRequest();
			}

			return null;
		}

		public bool IsValidReturnUrl(string returnUrl)
		{
			// To be considered valid, the return URL must be either local or have the
			// same origin as configured for this authorization server
			bool isLocalUrl = returnUrl.IsLocalUrl(),
				isAuthServerUrl = false;

			if (!isLocalUrl)
			{
				Uri returnUrlUri = new Uri(returnUrl),
					authServerUri = new Uri(_appConfigs.Value.AuthServerBaseUrl);

				int uriCompareResult = Uri.Compare(
					returnUrlUri,
					authServerUri,
					UriComponents.SchemeAndServer,
					UriFormat.Unescaped,
					StringComparison.CurrentCultureIgnoreCase
				);

				isAuthServerUrl = (uriCompareResult == 0);
			}

			if (!isLocalUrl && !isAuthServerUrl)
			{
				_logger.LogTrace("returnUrl is not valid");
				return false;
			}

			var index = returnUrl.IndexOf('?');
			if (index >= 0)
			{
				returnUrl = returnUrl.Substring(0, index);
			}

			if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
				returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
			{
				_logger.LogTrace("returnUrl is valid");
				return true;
			}

			return false;
		}







		public static class Constants
		{
			public static class AuthorizationParamsStore
			{
				public const string MessageStoreIdParameterName = "authzId";
			}
			public static class ProtocolRoutePaths
			{
				public const string Authorize = "connect/authorize";
				public const string AuthorizeCallback = Authorize + "/callback";
			}
		}




	}









	internal static class Extensions
	{

		[DebuggerStepThrough]
		public static bool IsLocalUrl(this string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return false;
			}

			// Allows "/" or "/foo" but not "//" or "/\".
			if (url[0] == '/')
			{
				// url is exactly "/"
				if (url.Length == 1)
				{
					return true;
				}

				// url doesn't start with "//" or "/\"
				if (url[1] != '/' && url[1] != '\\')
				{
					return true;
				}

				return false;
			}

			// Allows "~/" or "~/foo" but not "~//" or "~/\".
			if (url[0] == '~' && url.Length > 1 && url[1] == '/')
			{
				// url is exactly "~/"
				if (url.Length == 2)
				{
					return true;
				}

				// url doesn't start with "~//" or "~/\"
				if (url[2] != '/' && url[2] != '\\')
				{
					return true;
				}

				return false;
			}

			return false;
		}

		[DebuggerStepThrough]
		public static NameValueCollection ReadQueryStringAsNameValueCollection(this string url)
		{
			if (url != null)
			{
				var idx = url.IndexOf('?');
				if (idx >= 0)
				{
					url = url.Substring(idx + 1);
				}
				var query = QueryHelpers.ParseNullableQuery(url);
				if (query != null)
				{
					return query.AsNameValueCollection();
				}
			}

			return new NameValueCollection();
		}

		[DebuggerStepThrough]
		internal static AuthorizationRequest ToAuthorizationRequest(this ValidatedAuthorizeRequest request)
		{
			var authRequest = new AuthorizationRequest
			{
				Client = request.Client,
				RedirectUri = request.RedirectUri,
				DisplayMode = request.DisplayMode,
				UiLocales = request.UiLocales,
				IdP = request.GetIdP(),
				Tenant = request.GetTenant(),
				LoginHint = request.LoginHint,
				PromptModes = request.PromptModes,
				AcrValues = request.GetAcrValues(),
				ValidatedResources = request.ValidatedResources,
			};

			authRequest.Parameters.Add(request.Raw);
			foreach (var entry in request.RequestObjectValues)
				authRequest.RequestObjectValues.Add(entry.Key, entry.Value);

			return authRequest;
		}

		[DebuggerStepThrough]
		public static NameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
		{
			var nv = new NameValueCollection();

			foreach (var field in collection)
			{
				nv.Add(field.Key, field.Value.First());
			}

			return nv;
		}



		public static NameValueCollection FromFullDictionary(this IDictionary<string, string[]> source)
		{
			var nvc = new NameValueCollection();

			foreach ((string key, string[] strings) in source)
			{
				foreach (var value in strings)
				{
					nvc.Add(key, value);
				}
			}

			return nvc;
		}
	}
}