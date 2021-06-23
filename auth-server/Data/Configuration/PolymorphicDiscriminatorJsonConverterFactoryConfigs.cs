using System.Collections.Generic;
using System.Reflection;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>Configuration options for the operations of the <see cref="PolymorphicDiscriminatorJsonConverterFactory"/>.</summary>
	public class PolymorphicDiscriminatorJsonConverterFactoryConfigs
	{
		/// <summary>A list of extra assemblies to be parsed for polymorphic classes.</summary>
		/// <remarks>
		///     <para>
		///         The default type-initialization behavior for the <see cref="PolymorphicDiscriminatorJsonConverterFactory"/> class
		///         is to parse the assembly it is declared in, looking for types that use this JSON Converter Factory.
		///         It then builds a cache keyed by base type and discriminator pairs. For each of these pairs, there will be a value representing the
		///         corresponding/resulting derived class that is related to each pair.
		///     </para>
		///     <para>
		///         The <see cref="ExtraAssembliesToParse"/> configuration can be used to add more assemblies to be parsed in order to generate the
		///         aforementioned cache. This can be used, for example, to force the assembly for Integration Tests projects to be parsed for polymorphic
		///         classes that were declared for the purpose of the tests.
		///     </para>
		/// </remarks>
		public List<Assembly> ExtraAssembliesToParse { get; set; } = new List<Assembly>();
	}
}
