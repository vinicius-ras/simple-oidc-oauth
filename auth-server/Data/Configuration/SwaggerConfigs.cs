namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>Represents the configuration for Swagger - an OpenAPI implementation provided through the Swashbuckle library.</summary>
	public class SwaggerConfigs
	{
		/// <summary>A route template describing where is the OpenAPI Document located.</summary>
		/// <value>
		///     <p>
		///         The route template MUST contain a path segment referencing the <c>{documentName}</c> parameter.
		///         Swagger will use this parameter as the "URL-friendly name" to find the target OpenAPI Document.
		///     </p>
		///     <p>
		///         Notice that this application currently works with a single OpenAPI Document, and its "URL-friendly name"
		///         is defined via configuration (see <see cref="ApiDocumentNameUrlFriendly"/>).
		///     </p>
		///     <p>An example value would be <c>my-api-docs/{documentName}/openapi.json</c>.</p>
		/// </value>
		public string OpenApiDocumentRouteTemplate { get; set; }
		/// <summary>The title of the Swagger UI page, as it should appear in the user's browser.</summary>
		public string SwaggerUIPageTitle { get; set; }
		/// <summary>The route prefix for the Swagger UI.</summary>
		/// <remarks>
		///     This route prefix is used to define the URI the user should access in his/her browser in order to view
		///     the Swagger UI.
		/// </remarks>
		public string SwaggerUIRoutePrefix { get; set; }
		/// <summary>The full name of the API, as it should appear when displayed in the Web UI.</summary>
		public string ApiTitleFull { get; set; }
		/// <summary>The short name of the API, as it should appear when displayed in the Web UI.</summary>
		public string ApiTitleShort { get; set; }
		/// <summary>The version of the API.</summary>
		public string ApiVersion { get; set; }
		/// <summary>A description for the API to be displayed in the Swagger UI.</summary>
		public string ApiDescription { get; set; }
		/// <summary>The name of the OpenAPI Document, as it should appear when displayed in the Web UI.</summary>
		public string ApiDocumentName { get; set; }
		/// <summary>The name of the OpenAPI Document, specified in a URL-friendly format.</summary>
		public string ApiDocumentNameUrlFriendly { get; set; }
		/// <summary>The name to be displayed under the API's "Contact" information for users in the Swagger UI.</summary>
		public string ApiContactName { get; set; }
		/// <summary>The email to be displayed under the API's "Contact" information for users in the Swagger UI.</summary>
		public string ApiContactEmail { get; set; }
		/// <summary>The URL to be displayed under the API's "Contact" information for users in the Swagger UI.</summary>
		public string ApiContactUrl { get; set; }
		/// <summary>The name of the applied license to be displayed under the API's "License" information for users in the Swagger UI.</summary>
		public string ApiLicenseName { get; set; }
		/// <summary>The URL to be displayed under the API's "License" information for users in the Swagger UI.</summary>
		public string ApiLicenseUrl { get; set; }
		/// <summary>The URL to be displayed under the API's "Terms Of Service" information for users in the Swagger UI.</summary>
		public string ApiTermsOfServiceUrl { get; set; }
	}
}