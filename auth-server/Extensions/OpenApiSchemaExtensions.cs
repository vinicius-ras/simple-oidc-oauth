using System;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace SimpleOidcOauth.Extensions
{
	/// <summary>Extension methods for the <see cref="OpenApiSchema"/> class.</summary>
	public static class OpenApiSchemaExtensions
	{
		/// <summary>Given the name of a property contained in a <see cref="OpenApiSchema"/>, retrieves the <see cref="OpenApiSchema"/> describing that specific property.</summary>
		/// <param name="declaringOpenApiSchema">The <see cref="OpenApiSchema"/> which declares the target property.</param>
		/// <param name="propertyName">The name of the property whose <see cref="OpenApiSchema"/> reference is to be retrieved.</param>
		/// <returns>Returns the <see cref="OpenApiSchema"/> that describes the target property.</returns>
		/// <exception cref="ArgumentNullException">
		///     Thrown if any of the arguments is <c>null</c>, or if the <see cref="OpenApiSchema.Properties"/> property is null
		///     for the <paramref name="declaringOpenApiSchema"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">Thrown if any of the arguments is <c>null</c>, or if the </exception>
		public static OpenApiSchema GetPropertySchemaByCaseInsensitiveName(this OpenApiSchema declaringOpenApiSchema, string propertyName) =>
			declaringOpenApiSchema.Properties
				.First(prop => prop.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
				.Value;
	}
}
