using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SimpleOidcOauth.Data.Configuration;

namespace SimpleOidcOauth.Services
{
	/// <summary>Implementation for the <see href="IEmbeddedResourcesService" />.</summary>
	public class EmbeddedResourcesService : IEmbeddedResourcesService
	{
		// FIELDS
		/// <summary>Application's embedded resources configurations.</summary>
		private readonly EmbeddedResourcesConfigs _embeddedResourcesConfigs;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="appConfigs">Container-injected application configurations.</param>
		public EmbeddedResourcesService(IOptions<AppConfigs> appConfigs)
		{
			_embeddedResourcesConfigs = appConfigs.Value.EmbeddedResources;
		}




		// INTERFACE IMPLEMENTATION: IEmbeddedResourcesService
		/// <inheritdoc />
		public Stream GetResourceStream(string resourcePath)
		{
			var currentAssembly = this.GetType().Assembly;
			string embeddedResourcePath = (resourcePath == null)
				? null
				: string.Join(
					'.',
					_embeddedResourcesConfigs.ResourcesNamespace,
					resourcePath.Replace('/', '.')
				);
			return currentAssembly.GetManifestResourceStream(embeddedResourcePath);
		}


		/// <inheritdoc />
		public async Task<string> GetResourceAsStringAsync(string resourcePath, Encoding encoding)
		{
			using (var resourceStream = GetResourceStream(resourcePath))
			{
				if (resourceStream == null)
					return null;
				using (var streamReader = new StreamReader(resourceStream, encoding))
				{
					return await streamReader.ReadToEndAsync();
				}
			}
		}
	}
}