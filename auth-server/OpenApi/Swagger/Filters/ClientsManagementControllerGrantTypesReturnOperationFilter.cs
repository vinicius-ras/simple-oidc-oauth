using System.Linq;
using System.Net;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SimpleOidcOauth.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleOidcOauth.OpenApi.Swagger.Filters
{
	/// <summary>Enriches the OpenAPI documentation for the <see cref="ClientsManagementController.GetAllowedClientRegistrationGrantTypes"/> endpoint.</summary>
	public class ClientsManagementControllerGrantTypesOperationFilter : IOperationFilter
	{
		// INTERFACE IMPLEMENTATION: IOperationFilter
		/// <inheritdoc/>
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var successResponseExample = new OpenApiArray();
			successResponseExample.AddRange(new [] {
				new OpenApiString(IdentityModel.OidcConstants.GrantTypes.AuthorizationCode),
				new OpenApiString(IdentityModel.OidcConstants.GrantTypes.ClientCredentials),
				new OpenApiString(IdentityModel.OidcConstants.GrantTypes.Implicit),
			});

			var responseContents = operation.Responses[HttpStatusCode.OK.ToString("D")]
				.Content
				.Select(mediaTypeEntry => mediaTypeEntry.Value);
			foreach (var openApiMediaTypeEntry in responseContents)
				openApiMediaTypeEntry.Example = successResponseExample;
		}
	}
}
