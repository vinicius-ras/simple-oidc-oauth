using System;
using System.Net;

namespace SimpleOidcOauth.OpenApi.Swagger.Attributes
{
	/// <summary>Attribute used to document HTTP Response Headers for an OpenAPI Operation.</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class CustomResponseHeaderAttribute : Attribute
	{
		/// <summary>Indicates the HTTP Status Code associated to the return of the HTTP Response Header.</summary>
		public HttpStatusCode StatusCode { get; init; }
		/// <summary>The name of the HTTP Response Header which will be returned.</summary>
		public string HeaderName { get; init; }
		/// <summary>The <see cref="Type"/> of the HTTP Response Header which will be returned.</summary>
		/// <remarks>Currently, only the <see cref="string"/> is supported.</remarks>
		public Type HeaderType => typeof(string);
		/// <summary>A description for the HTTP Response Header which will be returned.</summary>
		public string Description { get; init; }
	}
}
