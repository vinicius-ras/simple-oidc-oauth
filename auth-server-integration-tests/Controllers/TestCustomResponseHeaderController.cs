using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SimpleOidcOauth.OpenApi.Swagger.Attributes;
using SimpleOidcOauth.OpenApi.Swagger.Filters;
using Swashbuckle.AspNetCore.Annotations;

namespace SimpleOidcOauth.Tests.Integration.Controllers
{
	/// <summary>
	///     A special controller whose sole purpose is to be used for testing the <see cref="CustomResponseHeaderOperationFilter"/>
	///     and <see cref="CustomResponseHeaderAttribute"/> classes.
	/// </summary>
	[ApiController]
	public class TestCustomResponseHeaderController : ControllerBase
	{
		// CONSTANTS
		/// <summary>The base endpoint where this controller's actions are contained.</summary>
		private const string BaseEndpoint = "/test-custom-response-header-controller-383cd4b3172a4d7db9e35f599e1d1dd2";
		/// <summary>Path to an endpoint which returns a documented "Location" header when it emits a HTTP Created (201) response.</summary>
		public const string SingleDocumentedLocationHeaderEndpoint = BaseEndpoint + "/single-documented-location-header-endpoint";
		/// <summary>The testable response's Status Code which is returned by the endpoint at <see cref="SingleDocumentedLocationHeaderEndpoint"/>.</summary>
		public const HttpStatusCode SingleDocumentedLocationHeaderStatusCode = HttpStatusCode.Created;
		/// <summary>The testable response's Header which is documented for the endpoint at <see cref="SingleDocumentedLocationHeaderEndpoint"/>.</summary>
		public const string SingleDocumentedLocationHeaderName = nameof(HeaderNames.Location);
		/// <summary>The description of the header specified by <see cref="SingleDocumentedLocationHeaderName"/>.</summary>
		public const string SingleDocumentedLocationHeaderDescription = "Random Description 53fd5e4697fa47158e904fbf309db502";
		/// <summary>Path to an endpoint which does not contain any extra documentation for returned headers.</summary>
		public const string NoDocumentedHeadersEndpoint = BaseEndpoint + "/no-documented-headers-endpoint";
		/// <summary>Path to an endpoint which contains extra documentation for multiple custom headers.</summary>
		public const string MultipleDocumentedHeadersEndpoint = BaseEndpoint + "/multiple-documented-headers-endpoint";
		/// <summary>Name of the first custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderName1 = "custom-header-876efcdccdaf4e70a7f51f341bd6df73";
		/// <summary>Name of the second custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderName2 = "custom-header-7172b1ead1794816bc84a3ac32f40d1c";
		/// <summary>Name of the third custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderName3 = "custom-header-cc17d9d5a7b5409383299a42153e0870";
		/// <summary>Name of the fourth custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderName4 = "custom-header-52b182e6d9f44b3c886cb495a0cb6280";
		/// <summary>Description for the first custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderDescription1 = "Custom header description af596156ee6c494b846e2666d958d913";
		/// <summary>Description for the second custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderDescription2 = "Custom header description 9869c844168b44ae945b640be639f6c6";
		/// <summary>Description for the third custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderDescription3 = "Custom header description 206ca0ed5ca742c39f155bf895a00eea";
		/// <summary>Description for the fourth custom header returned by the endpoint at path <see cref="MultipleDocumentedHeadersEndpoint"/>.</summary>
		public const string CustomHeaderDescription4 = "Custom header description 135bcf3b2c1d4e2197714d1698cbdad3";





		// INSTANCE METHODS
		/// <summary>A sample endpoint where only one header is documented.</summary>
		/// <returns>The returned response is fully ignorable.</returns>
		/// <response code="201">All good.</response>
		[Route(SingleDocumentedLocationHeaderEndpoint)]
		[HttpGet]
		[CustomResponseHeader(
			StatusCode = SingleDocumentedLocationHeaderStatusCode,
			Description = SingleDocumentedLocationHeaderDescription,
			HeaderName = SingleDocumentedLocationHeaderName)]
		[ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
		public void SingleDocumentedLocationHeaderAction() {}


		/// <summary>A sample endpoint where there are no documented headers.</summary>
		/// <returns>The returned response is fully ignorable.</returns>
		/// <response code="200">All went well.</response>
		/// <response code="401">User must be authenticated.</response>
		/// <response code="403">User does not have access to this endpoint.</response>
		[Route(NoDocumentedHeadersEndpoint)]
		[HttpGet]
		[ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
		public void NoDocumentedHeadersAction() {}


		/// <summary>A sample endpoint where there are multiple documented headers.</summary>
		/// <returns>The returned response is fully ignorable.</returns>
		/// <response code="200">
		///     All went well.
		///     Returns custom headers <see cref="CustomHeaderName1"/>, <see cref="CustomHeaderName2"/>, and <see cref="CustomHeaderName3"/>.
		/// </response>
		/// <response code="401">User must be authenticated.</response>
		/// <response code="403">
		///     User does not have access to this endpoint.
		///     Returns custom header <see cref="CustomHeaderName4"/>.
		/// </response>
		[Route(MultipleDocumentedHeadersEndpoint)]
		[HttpDelete]
		[CustomResponseHeader(
			StatusCode = HttpStatusCode.OK,
			Description = CustomHeaderDescription1,
			HeaderName = CustomHeaderName1)]
		[CustomResponseHeader(
			StatusCode = HttpStatusCode.OK,
			Description = CustomHeaderDescription2,
			HeaderName = CustomHeaderName2)]
		[CustomResponseHeader(
			StatusCode = HttpStatusCode.OK,
			Description = CustomHeaderDescription3,
			HeaderName = CustomHeaderName3)]
		[CustomResponseHeader(
			StatusCode = HttpStatusCode.Forbidden,
			Description = CustomHeaderDescription4,
			HeaderName = CustomHeaderName4)]
		[ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
		public void MultipleDocumentedHeadersAction() {}
	}
}