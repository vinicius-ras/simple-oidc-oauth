using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Services
{
	/// <summary>
	///     A service for accessing resources that have been embedded within the application's assemblies.
	///     These resources are located in the "EmbeddedResources" folder of the project.
	/// </summary>
	public interface IEmbeddedResourcesService
	{
		/// <summary>Retrieves a <see cref="Stream" /> to access the binary contents of the given resource.</summary>
		/// <param name="resourcePath">
		///     The location of the resource to be accessed, given as a path relative to the main embedded resources folder.
		///     Forward slashes ('/') should be used to separate path segments, if applicable.
		/// </param>
		/// <returns>
		///     <para>
		///         In case of success, returns the <see cref="Stream" /> object that can be used to access the given resource.
		///         The returned <see cref="Stream" /> should be disposed of after being used.
		///     </para>
		///     <para>In case of failure, returns the <c>null</c>.</para>
		/// </returns>
		/// <exception cref="ArgumentNullException">The <paramref name="resourcePath"/> parameter is null.</exception>
		/// <exception cref="NotImplementedException">The resource's length is greater than <see cref="Int64.MaxValue" />.</exception>
		Stream GetResourceStream(string resourcePath);


		/// <summary>Retrieves a string representing a specific text resource.</summary>
		/// <param name="resourcePath">
		///     The location of the resource to be accessed, given as a path relative to the main embedded resources folder.
		/// </param>
		/// <param name="encoding">
		///     The encoding used to read the resource.
		///     If not specified, <see cref="Encoding.UTF8" /> will be used.
		/// </param>
		/// <returns>
		///     <para>In case of success, returns the contents of the given resource, read as a string.</para>
		///     <para>In case of failure, returns the <c>null</c>.</para>
		/// </returns>
		/// <exception cref="ArgumentNullException">The <paramref name="resourcePath"/> parameter is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The number of characters in the resource is larger than <see cref="Int32.MaxValue" />.</exception>
		/// <exception cref="NotImplementedException">The resource's length is greater than <see cref="Int64.MaxValue" />.</exception>
		Task<string> GetResourceAsStringAsync(string resourcePath, Encoding encoding = null);
	}
}