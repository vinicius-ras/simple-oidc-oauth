using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IdentityServer4.Models;
using SimpleOidcOauth.OpenApi.Swagger.Filters;
using Swashbuckle.AspNetCore.Annotations;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a resource (API Scope, API Resource, or Identity Resource).</summary>
	/// <remarks>This class represents a serializable version of the <see cref="Resource"/> class.</remarks>
	[SwaggerSchemaFilter(typeof(SerializableResourceSchemaFilter))]
	public abstract class SerializableResource : IPolymorphicDiscriminator
	{
		// INSTANCE PROPERTIES
		/// <summary>Indicates if this resource is enabled.</summary>
		public bool Enabled { get; set; }
		/// <summary>The unique name of the resource.</summary>
		/// <example>my-petshop-api</example>
		[Required]
		public string Name { get; set; }
		/// <summary>Display name of the resource.</summary>
		/// <example>Foobar Petshop API</example>
		public string DisplayName { get; set; }
		/// <summary>Description of the resource.</summary>
		/// <example>A web API for managing petshops.</example>
		public string Description { get; set; }
		/// <summary>Specifies whether this scope is shown in the discovery document.</summary>
		public bool ShowInDiscoveryDocument { get; set; }
		/// <summary>
		///     List of associated user claims that should be included when this resource is
		///     requested.
		/// </summary>
		public IEnumerable<string> UserClaims { get; set; }
		/// <summary>Gets or sets the custom properties for the resource.</summary>
		/// <value>The properties.</value>
		public IDictionary<string, string> Properties { get; set; }





		// INTERFACE IMPLEMENTATION: IPolymorphicDiscriminator
		/// <inheritdoc />
		public abstract string DiscriminatorValue { get; }
	}
}