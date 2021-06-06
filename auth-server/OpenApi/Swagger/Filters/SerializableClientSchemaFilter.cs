using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleOidcOauth.OpenApi.Swagger.Filters
{
	/// <summary>A Swagger schema filter which improves documentation for the <see cref="SerializableClient"/> schema.</summary>
	public class SerializableClientSchemaFilter : ISchemaFilter
	{
		// INTERFACE IMPLEMENTATION: ISchemaFilter
		/// <inheritdoc/>
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			// Enhance the "Allowed CORS Origins" property examples
			var propAllowedCorsOrigins = schema.GetPropertySchemaByCaseInsensitiveName(nameof(SerializableClient.AllowedCorsOrigins));

			var allowedCorsOriginsExamples = new OpenApiArray();
			propAllowedCorsOrigins.Example = allowedCorsOriginsExamples;
			allowedCorsOriginsExamples.AddRange(new[] {
				new OpenApiString("https://my-fake-app.com"),
				new OpenApiString("http://localhost:4321"),
			});


			// Enhance the "Allowed CORS Origins" property examples
			var propPostLogoutRedirectUris = schema.GetPropertySchemaByCaseInsensitiveName(nameof(SerializableClient.PostLogoutRedirectUris));

			var PostLogoutRedirectUrisExamples = new OpenApiArray();
			propPostLogoutRedirectUris.Example = PostLogoutRedirectUrisExamples;
			PostLogoutRedirectUrisExamples.AddRange(new[] {
				new OpenApiString("https://my-fake-app.com/sign-out-callback"),
				new OpenApiString("http://localhost:4321/logged-out-page"),
			});


			// Enhance the "Allowed CORS Origins" property examples
			var propRedirectUris = schema.GetPropertySchemaByCaseInsensitiveName(nameof(SerializableClient.RedirectUris));

			var RedirectUrisExamples = new OpenApiArray();
			propRedirectUris.Example = RedirectUrisExamples;
			RedirectUrisExamples.AddRange(new[] {
				new OpenApiString("https://my-fake-app.com/sign-in-callback"),
				new OpenApiString("http://localhost:4321/welcome-page"),
			});


			// Enhance the "Allowed Grant Types" property examples
			var propAllowedGrantTypes = schema.GetPropertySchemaByCaseInsensitiveName(nameof(SerializableClient.AllowedGrantTypes));

			var allowedGrantTypesExamples = new OpenApiArray();
			propAllowedGrantTypes.Example = allowedGrantTypesExamples;
			allowedGrantTypesExamples.AddRange(new[] {
				new OpenApiString("authorization_code"),
				new OpenApiString("client_credentials"),
				new OpenApiString("implicit"),
			});


			// Enhance the "Allowed Scopes" property examples
			var propAllowedScopes = schema.GetPropertySchemaByCaseInsensitiveName(nameof(SerializableClient.AllowedScopes));

			var allowedScopesExamples = new OpenApiArray();
			propAllowedScopes.Example = allowedScopesExamples;
			allowedScopesExamples.AddRange(new[] {
				new OpenApiString("openid"),
				new OpenApiString("profile"),
				new OpenApiString("email"),
				new OpenApiString("my-api-1"),
				new OpenApiString("my-api-2.read-only"),
			});
		}
	}
}