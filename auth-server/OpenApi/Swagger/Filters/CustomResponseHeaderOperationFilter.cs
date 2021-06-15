using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SimpleOidcOauth.OpenApi.Swagger.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleOidcOauth.OpenApi.Swagger.Filters
{
	/// <summary>Used to add a custom HTTP Response Header to the description of OpenAPI Operations' responses.</summary>
	public class CustomResponseHeaderOperationFilter : IOperationFilter
	{
		// INSTANCE FIELDS
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</summary>
		private readonly ILogger<CustomResponseHeaderOperationFilter> _logger;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		public CustomResponseHeaderOperationFilter(ILogger<CustomResponseHeaderOperationFilter> logger)
		{
			_logger = logger;
		}





		// INTERFACE IMPLEMENTATION: IOperationFilter
		/// <inheritdoc/>
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (context?.MethodInfo == null)
				return;

			// Retrieves all [CustomResponseHeader] attributes from the method and its declaring types
			var controllerCustomResponseHeaders = context.MethodInfo
				.DeclaringType
				.GetCustomAttributes(true)
				.OfType<CustomResponseHeaderAttribute>();
			var operationCustomResponseHeaders = context.MethodInfo
				.GetCustomAttributes(true)
				.OfType<CustomResponseHeaderAttribute>()
				.Union(controllerCustomResponseHeaders);
			foreach (var customResponseHeader in operationCustomResponseHeaders)
			{
				// Verify if the operation actually declares the specified Status Code as a possibly returned result
				string responseStatusCodeStr = customResponseHeader.StatusCode.ToString("D");
				if (operation.Responses.TryGetValue(responseStatusCodeStr, out var targetResponse) == false)
				{
					// The target operation is annotated with a [CustomResponseHeader] which specifies an HTTP Status Code which does not
					// have a corresponding <response code="xxx"/> declaration in the operation's documentation.
					string operationName = $"{context.MethodInfo.DeclaringType.Name}.{context.MethodInfo.Name}",
						attributeName = nameof(CustomResponseHeaderAttribute),
						allOperationStatusCodes = string.Join(
							", ",
							operation.Responses
								.Keys
								.OrderBy(responseStr => responseStr)
								.Select(responseStr => $@"""{responseStr}""")
						);
					_logger.LogWarning(
						""
						+ $@"Operation ""{operationName}"" has a ""{attributeName}"" attribute applied "
						+ $@"to it without a corresponding response with status code ""{responseStatusCodeStr}"". "
						+ $@"This operation only returns the following responses: {allOperationStatusCodes}."
					);
					continue;
				}


				// Add the header description, emitting a warning if it already exists
				targetResponse.Headers = targetResponse.Headers ?? new Dictionary<string, OpenApiHeader>();
				if (targetResponse.Headers.ContainsKey(customResponseHeader.HeaderName))
				{
					_logger.LogWarning(
						$@"Operation ""{context.MethodInfo.DeclaringType.Name}.{context.MethodInfo.Name}"" has a ""{nameof(CustomResponseHeaderAttribute)}"" applied "
						+ $@"to it, but the target header already has a description."
					);
					continue;
				}

				string typeName = customResponseHeader.HeaderType.Name;
				if (typeName.Equals(nameof(String)))
					typeName = nameof(String).ToLower();
				targetResponse.Headers[customResponseHeader.HeaderName] = new OpenApiHeader
				{
					Description = customResponseHeader.Description,
					Schema = new OpenApiSchema{
						Type = typeName,
					}
				};
			}
		}
	}
}