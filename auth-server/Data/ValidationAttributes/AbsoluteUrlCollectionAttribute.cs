using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SimpleOidcOauth.Data.Configuration;

namespace SimpleOidcOauth.Data.ValidationAttributes
{
	/// <summary>Validates an <see cref="IEnumerable{T}"/> collection containing strings representing absolute HTTP/HTTPS URLs.</summary>
	public class AbsoluteUrlCollectionAttribute : ValidationAttribute
	{
		// CONSTANTS
		/// <summary>The error message to be used when the validation fails.</summary>
		private const string ErrorMessageValue = "One or more of the given absolute URLs are not valid.";





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		public AbsoluteUrlCollectionAttribute()
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
			var absoluteUrlsCollection = value as IEnumerable<string>;
			if (absoluteUrlsCollection == null)
				return false;

			// Validate each url
			foreach (string absoluteUrl in absoluteUrlsCollection)
			{
				Uri uri;
				try
				{
					uri = new Uri(absoluteUrl, UriKind.Absolute);
				}
				catch (FormatException)
				{
					return false;
				}

				// Validate the accepted schemes for the IdP (currently accepted schemes are limited to HTTP and HTTPS)
				bool uriHasAcceptableScheme = AppConfigs.AcceptableClientRedirectionUrlSchemes
					.Any(acceptableSchemeName => string.Equals(uri.Scheme, acceptableSchemeName, StringComparison.OrdinalIgnoreCase));
				if (uriHasAcceptableScheme == false)
					return false;
			}

			return true;
		}
	}
}