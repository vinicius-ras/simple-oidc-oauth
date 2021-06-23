using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SimpleOidcOauth.Data.Configuration;

namespace SimpleOidcOauth.Data.ValidationAttributes
{
	/// <summary>Validates an <see cref="IEnumerable{T}"/> collection containing strings representing HTTP/HTTPS URL origins.</summary>
	public class UrlOriginCollectionAttribute : ValidationAttribute
	{
		// CONSTANTS
		/// <summary>The error message to be used when the validation fails.</summary>
		private const string ErrorMessageValue = "One or more of the given origins are not valid.";





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		public UrlOriginCollectionAttribute()
			: base(errorMessage: ErrorMessageValue)
		{
		}





		// OVERRIDDEN METHODS: ValidationAttribute
		/// <inheritdoc />
		public override bool IsValid(object value)
		{
			// Null values are always valid (use [Required] to invalidate null fields, as per good validation practices)
			if (value == null)
				return true;

			// This validation attribute is only applyable to a collection of strings
			var originsCollection = value as IEnumerable<string>;
			if (originsCollection == null)
				return false;

			// Validate each origin
			foreach (string origin in originsCollection)
			{
				Uri originUri;
				try
				{
					originUri = new Uri(origin, UriKind.Absolute);
				}
				catch (FormatException)
				{
					return false;
				}

				// Validate the accepted schemes for the IdP (currently accepted schemes are limited to HTTP and HTTPS)
				bool uriHasAcceptableScheme = AppConfigs.AcceptableClientRedirectionUrlSchemes
					.Any(acceptableSchemeName => string.Equals(originUri.Scheme, acceptableSchemeName, StringComparison.OrdinalIgnoreCase));
				if (uriHasAcceptableScheme == false)
					return false;

				// Origins are URIs without path/query/fragment segments, and not ending in "/" (their format is basically: "<scheme>://[username[:password]@]<domain>[:port]")
				bool pathAndQueryEmpty = (string.IsNullOrEmpty(originUri.PathAndQuery))
					|| (originUri.PathAndQuery == "/" && originUri.OriginalString.EndsWith("/") == false);
				bool fragmentEmpty = (string.IsNullOrEmpty(originUri.Fragment));
				if (pathAndQueryEmpty == false || fragmentEmpty == false)
					return false;
			}

			return true;
		}
	}
}