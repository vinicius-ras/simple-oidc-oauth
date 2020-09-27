using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	///     Extension class which encapsulate configurations to allow cookies to be set with
	///     a "SameSite" attribute configured with a "None" value.
	/// </summary>
	/// <remarks>
	///     <para>
	///         Chrome 80+ introduced a breaking change to how the "SameSite" attribute of a cookie works
	///         by default. The default value for this attribute changed to "SameSite=Lax", while it
	///         traditionally has been treated by most browsers as "SameSite=None". Additionally many
	///         older browsers do not support setting "SameSite=None": they only support the values "Lax"
	///         and "Strict", and treat unknown/unsupported values as "SameSite=Strict".
	///     </para>
	///     <para>
	///         This breaking change deeply affects how a silent token refreshing happens when using a Single
	///         Page Applications (SPAs) to refresh OpenID Connect tokens.
	///     </para>
	///     <para>
	///         This extension class configures a Cookie Policy which mitigates that breaking change, by
	///         effectively sniffing the client's User Agent, and setting the cookies' SameSite attribute
	///         according to that User Agent.
	///     </para>
	/// </remarks>
	public static class SameSiteCookiesExtensions
	{
		/// <summary>
		///     A collection specifying which User Agents do not support the "SameSite=None"
		///     cookie attribute.
		/// </summary>
		/// <value>
		///     A collection where each element is a set of strings indicating text pieces that must ALL be
		///     present in the User-Agent request's header in order for the client's user agent to be
		///     considered as an agent that doesn't support the "SameSite=None" cookie attribute.
		/// </value>
		private static readonly string[][] _userAgentsNotSupportingSameSiteNone = {
			// iOS 12 browsers
			new [] {"CPU iPhone OS 12"},
			new [] {"iPad; CPU OS 12"},

			// Mac OS X browsers (all of the terms in that set must be present in the User-Agent header)
			new [] {"Safari", "Macintosh; Intel Mac OS X 10_14", "Version/"},

			// Chrome 50-69
			new [] {"Chrome/5"},
			new [] {"Chrome/6"},
		};





		/// <summary>
		///     Configures a cookie policy which mitigates the problem of specific older browsers that do not
		///     correctly support setting cookies with the "SameSite=None" attribute.
		/// </summary>
		/// <param name="services">
		///     The collection of host services, where cookie policy configurations will be performed.
		/// </param>
		/// <returns>
		///     Returns the same <paramref name="services"/> collection that have been passed ot this method.
		/// </returns>
		public static IServiceCollection ConfigureSameSiteCookies(this IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(opts => {
				opts.MinimumSameSitePolicy = SameSiteMode.Unspecified;
				opts.OnAppendCookie = appendCookieCtx => ProcessSameSiteCookie(appendCookieCtx.CookieOptions, appendCookieCtx.Context);
				opts.OnDeleteCookie = deleteCookieCtx => ProcessSameSiteCookie(deleteCookieCtx.CookieOptions, deleteCookieCtx.Context);
			});
			return services;
		}


		/// <summary>
		///     Utility method which processes a cookie that is to be appended/deleted to a response for a
		///     client, modifying the cookie if it has the "SameSite=None" attribute while client's User Agent
		///     doesn't support that attribute.
		/// </summary>
		/// <param name="cookieOpts">
		///     The options that have been configured for the cookie that is being processed.
		/// </param>
		/// <param name="httpContext">
		///     The HTTP context used to obtain information about the client's request.
		/// </param>
		private static void ProcessSameSiteCookie(CookieOptions cookieOpts, HttpContext httpContext)
		{
			// Does the cookie being processed set a "SameSite" attribute to "None"?
			if (cookieOpts.SameSite == SameSiteMode.None)
			{
				// Verify if the client's user agent really supports "SameSite=None" on a cookie
				string clientUserAgent = httpContext.Request.Headers["User-Agent"].ToString();

				bool isSameSiteNoneUnsupportedByUserAgent = _userAgentsNotSupportingSameSiteNone
					.Any(agentsSet => agentsSet.All(agentNamePart => clientUserAgent.Contains(agentNamePart, StringComparison.OrdinalIgnoreCase)));


				// If the client's User Agent doesn't support "SameSite=None", let's simply set this attribute
				// to "Unspecified", which makes our server NOT send the "SameSite" attribute along with the
				// cookie.
				if (isSameSiteNoneUnsupportedByUserAgent)
					cookieOpts.SameSite = SameSiteMode.Unspecified;
			}
		}
	}
}