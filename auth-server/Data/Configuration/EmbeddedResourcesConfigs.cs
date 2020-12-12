namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>
	///     Models the configurations for the service which allows access to embedded resources (<see cref="Services.IEmbeddedResourcesService"/>).
	/// </summary>
	public class EmbeddedResourcesConfigs
	{
		/// <summary>The namespace to be used as the "root namespace" for all of the resources.</summary>
		/// <value>
		///     A namespace value which consists of the application's "root namespace", followed by a dot
		///     and the name of the project's folder which contains all of the project's embedded resources.
		/// </value>
		public string ResourcesNamespace { get; set; }
	}
}