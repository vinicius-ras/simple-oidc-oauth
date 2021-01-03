using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SimpleOidcOauth.Tests.Integration.Utilities
{
	/// <summary>
	///     Utility class providing methods that deal with web components, such as <see cref="HttpClient"/> extension methods,
	///     URI internal components (query strings, fragments, etc), and other components alike.
	/// </summary>
	static class WebUtilities
	{
		// STATIC METHODS
		/// <summary>
		///     Parses a received HTTP Redirection response, extracting a specific value from the query parameters of the
		///     target redirection location's URL.
		/// </summary>
		/// <param name="redirectResponse">The HTTP redirection response whose "Location" header will be parsed.</param>
		/// <param name="queryParameterName">The name of the query parameter to be extracted from the "Location" header's URL.</param>
		/// <returns>
		///     <para>In case of success, returns the extracted query parameter's value.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		public static string ReadRedirectLocationQueryParameter(HttpResponseMessage redirectResponse, string queryParameterName)
		{
			string locationQueryString = redirectResponse?.Headers?.Location?.Query;
			if (locationQueryString == null)
				return null;

			var authorizeRequestResponseQueryParams = HttpUtility.ParseQueryString(locationQueryString);
			return authorizeRequestResponseQueryParams[queryParameterName];
		}


		/// <summary>
		///     Parses a received HTTP Redirection response, extracting a specific value from the fragment part of the
		///     target redirection location's URL.
		/// </summary>
		/// <param name="redirectResponse">The HTTP redirection response whose "Location" header will be parsed.</param>
		/// <param name="fragmentParameterName">The name of the fragment parameter to be extracted from the "Location" header's URL.</param>
		/// <returns>
		///     <para>In case of success, returns the extracted fragment parameter's value.</para>
		///     <para>In case of failure, returns <c>null</c>.</para>
		/// </returns>
		public static string ReadRedirectLocationFragmentParameter(HttpResponseMessage redirectResponse, string fragmentParameterName)
		{
			string locationFragmentString = redirectResponse?.Headers?.Location?.Fragment?.TrimStart('#');
			if (locationFragmentString == null)
				return null;

			var authorizeRequestResponseQueryParams = HttpUtility.ParseQueryString(locationFragmentString);
			return authorizeRequestResponseQueryParams[fragmentParameterName];
		}





		// EXTENSION METHODS: HttpClient
		/// <summary>
		///     For <see cref="HttpClient"/> instances configured not to perform redirection automatically, this method can be called
		///     to actually perform the redirection, given the HTTP redirection response.
		/// </summary>
		/// <param name="httpClient">The client to be used for the redirection procedure.</param>
		/// <param name="redirectMessage">The HTTP redirection response which will be followed by the <see cref="HttpClient"/>.</param>
		/// <returns>
		///     Returns a <see cref="Task{TResult}"/> representing a call to <see cref="HttpClient.GetAsync(Uri?)"/>, with an argument
		///     representing the target location of the HTTP redirection response.
		/// </returns>
		public static Task<HttpResponseMessage> ManuallyFollowRedirectResponse(this HttpClient httpClient, HttpResponseMessage redirectMessage)
		{
			// Check the redirection parameters
			var messageStatusCode = redirectMessage?.StatusCode;
			if (messageStatusCode == null || (int)messageStatusCode < 300 || (int)messageStatusCode >= 400)
				throw new InvalidOperationException($"Failed to redirect: argument {nameof(redirectMessage)} is an HTTP response with status code {messageStatusCode} outside of the range 300-399.");

			var redirectionTargetLocation = redirectMessage?.Headers?.Location;
			if (redirectionTargetLocation == null)
				throw new InvalidOperationException($@"Failed to redirect: argument {nameof(redirectMessage)} is an HTTP redirection response without a ""{nameof(HttpResponseMessage.Headers.Location)}"" header.");

			// Extract the redirection's target location and return a task which performs the redirection
			return httpClient.GetAsync(redirectionTargetLocation);
		}
	}
}