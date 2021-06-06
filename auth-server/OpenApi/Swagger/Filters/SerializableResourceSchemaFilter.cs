using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleOidcOauth.OpenApi.Swagger.Filters
{
	/// <summary>A Swagger schema filter which improves documentation for the <see cref="SerializableResource"/> schema.</summary>
	public class SerializableResourceSchemaFilter : ISchemaFilter
	{
		// INTERFACE IMPLEMENTATION: ISchemaFilter
		/// <inheritdoc/>
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			// Enhance the "Allowed CORS Origins" property examples
			var propUserClaims = schema.GetPropertySchemaByCaseInsensitiveName(nameof(SerializableResource.UserClaims));

			var userClaimsExamples = new OpenApiArray();
			propUserClaims.Example = userClaimsExamples;
			userClaimsExamples.AddRange(new[] {
				new OpenApiString("openid"),
				new OpenApiString("profile"),
				new OpenApiString("email"),
				new OpenApiString("phone"),
				new OpenApiString("full_name"),
				new OpenApiString("birthdate"),
				new OpenApiString("foobar-identification-card"),
			});

			// Enhance the "Additional Properties" property examples
			var propAdditionalProperties = schema.GetPropertySchemaByCaseInsensitiveName(nameof(SerializableResource.Properties));

			var additionalPropertiesExample = new OpenApiObject
			{
				["myFavoriteNumber"] = new OpenApiString("1234"),
				["someArbitraryProperty"] = new OpenApiString("potato"),
			};
			propAdditionalProperties.Example = additionalPropertiesExample;
		}
	}
}