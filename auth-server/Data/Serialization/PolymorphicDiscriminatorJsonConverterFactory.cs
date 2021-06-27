using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleOidcOauth.Data.Configuration;

namespace SimpleOidcOauth.Data.Serialization
{
    /// <summary>A factory of JSON Converter objects which are able to handle polymorphic classes.</summary>
    /// <remarks>
    ///     <para>
    ///         All classes which implement <see cref="IPolymorphicDiscriminator"/> will use JSON Converters produced by this factory class for
    ///         both serialization and deserialization.
    ///     </para>
    ///     <para>
    ///         Subclasses should override the <see cref="IPolymorphicDiscriminator.DiscriminatorValue"/> field, specifying a value for it which
    ///         indicates the string identifier to be used in the JSON payload to identify the target derived type during serialization/deserialization.
    ///     </para>
    /// </remarks>
	public class PolymorphicDiscriminatorJsonConverterFactory : JsonConverterFactory
	{
        // CONSTANTS
        /// <summary>The name of the property of the JSON Object which is expected to be used as a discriminator property.</summary>
        public const string DiscriminatorPropertyName = "$type";





        // STATIC FIELDS
        /// <summary>
        ///     Maps the different combinations of base types and discriminator values to the respective derived types of instances
        ///     these combinations are expected to generate.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This cache is built by searching for base classes which implement the <see cref="IPolymorphicDiscriminator"/> interface.
        ///         The search is performed by default on the same assembly where the <see cref="PolymorphicDiscriminatorJsonConverterFactory"/>
        ///         class is declared.
        ///     </para>
        ///     <para>
        ///         Extra assemblies can be specified for searching for classes by configuring this class
        ///         via the <see cref="PolymorphicDiscriminatorJsonConverterFactoryConfigs.ExtraAssembliesToParse"/> option.
        ///         This can be used in scenarios like Unit/Integration Tests, where there is a need to add custom types for testing purposes.
        ///     </para>
        /// </remarks>
        private static Dictionary<TypeDiscriminatorKey, Type> _typeDiscriminatorsCache = new Dictionary<TypeDiscriminatorKey, Type>();





        // INSTANCE METHODS
        /// <summary>Constructor.</summary>
        /// <param name="configs">Object containing the configurations to be applied to the factory and its generated converters.</param>
        public PolymorphicDiscriminatorJsonConverterFactory(PolymorphicDiscriminatorJsonConverterFactoryConfigs configs = default)
        {
            lock (_typeDiscriminatorsCache)
            {
                // The cache should only be initialized once per application run
                if (_typeDiscriminatorsCache.Count > 0)
                    return;

                // Find and process all concrete Derived-Types which implement the IPolymorphicDiscriminator interface.
                // NOTE: this search also includes nested types.
                var executingAssembly = Assembly.GetExecutingAssembly();

                var assembliesToCheck = new List<Assembly>();
                assembliesToCheck.Add(executingAssembly);
                assembliesToCheck.AddRange(configs?.ExtraAssembliesToParse ?? Enumerable.Empty<Assembly>());

                var allPubliclyAccessibleProjectTypes = new HashSet<Type>();
                var typesToProcessQueue = new Queue<Type>(assembliesToCheck.SelectMany(asm => asm.GetTypes()));
                while (typesToProcessQueue.Count > 0)
                {
                    var currentType = typesToProcessQueue.Dequeue();
                    allPubliclyAccessibleProjectTypes.Add(currentType);

                    foreach (var nestedType in currentType.GetNestedTypes())
                        typesToProcessQueue.Enqueue(nestedType);
                }

                var polymorphicConcreteTypes = allPubliclyAccessibleProjectTypes
                    .Where(type =>
                        type.IsAbstract == false
                        && type.IsAssignableTo(typeof(IPolymorphicDiscriminator)));


                // Use the found concrete types to build a cache of base types and discriminator values which are associated to specific concrete-types during JSON [de]serialization
                foreach (var concreteType in polymorphicConcreteTypes)
                {
                    // Starting from the current type, find the base-type which inherits directly from the "System.Object" type
                    Type superType;
                    for (superType = concreteType; superType != null && superType.BaseType != typeof(object); superType = superType.BaseType)
                        ;

                    // Create an instance of the target derived-type and use it to fetch the derived-type's "discriminator value"
                    var instance = (IPolymorphicDiscriminator) Activator.CreateInstance(concreteType);
                    var discriminatorValue = instance.DiscriminatorValue;

                    // Insert a new entry into our cache
                    var cacheKey = new TypeDiscriminatorKey
                    {
                        BaseType = superType,
                        DiscriminatorValue = discriminatorValue,
                    };
                    _typeDiscriminatorsCache.Add(cacheKey, concreteType);
                }
            }
        }





        // INTERFACE IMPLEMENTATION: JsonConverterFactory
        /// <inheritdoc/>
		public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsAssignableTo(typeof(IPolymorphicDiscriminator))
            && _typeDiscriminatorsCache.Any(entry => entry.Key.BaseType == typeToConvert);


        /// <inheritdoc/>
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var converterType = typeof(PolymorphicDiscriminatorJsonConverter<>)
                .MakeGenericType(typeToConvert);
            var converterInstance = (JsonConverter) Activator.CreateInstance(converterType);
            return converterInstance;
		}





        // NESTED TYPES
        /// <summary>A composite type of key, formed by a base <see cref="Type"/> and a discriminator value.</summary>
        /// <remarks>
        ///     Composite keys of this type are used by the <see cref="_typeDiscriminatorsCache"/> map. They form a pair composed by a base <see cref="Type"/> and a discriminator
        ///     value. This pair is used to discover what type of Derived-<see cref="Type"/> should be generated when trying to serialize/deserialize a specific
        ///     Base-<see cref="Type"/> while using some target discriminator value.
        /// </remarks>
        [DebuggerDisplay(@"Type {BaseType.Name} + Discriminator {DiscriminatorValue}")]
        private struct TypeDiscriminatorKey
        {
            /// <summary>The Base-<see cref="Type"/> of the object that needs to be serialized/deserialized.</summary>
            public Type BaseType { get; set; }
            /// <summary>The discriminator value which will dictate the resulting Derived-<see cref="Type"/>.</summary>
            public string DiscriminatorValue { get; set; }
        }


        /// <summary>A JSON Converter which is able to handle polymorphism while serializing/deserializing base types to/from JSON.</summary>
        /// <remarks>
        ///     <para>
        ///         The converter expects the base classes which use it to implement the <see cref="IPolymorphicDiscriminator"/> interface.
        ///         This interface allows for the specification of "discriminator values" that will be used to differentiate between the different subtypes of a given base class.
        ///     </para>
        ///     <para>
        ///         Discriminator values are serialized to JSON contents and placed as values for a special property, whose key name is currently fixed and
        ///         specified by <see cref="DiscriminatorPropertyName"/>.
        ///     </para>
        /// </remarks>
        /// <typeparam name="TBaseClass">The polymorphic base type for which serialization/deserialization will be provided via a discriminator property.</typeparam>
        private class PolymorphicDiscriminatorJsonConverter<TBaseClass> : JsonConverter<TBaseClass>
            where TBaseClass : class, IPolymorphicDiscriminator
        {
            // OVERRIDDEN METHODS: JsonConverter<T>
            /// <inheritdoc />
            public override void Write(Utf8JsonWriter writer, TBaseClass value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                // Iterate over all public instance properties, but placing the "discriminator value" property as the first one of the list (for organization purposes only)
                var allProperties = value.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var discriminatorProperty = allProperties.Single(prop => prop.Name == nameof(IPolymorphicDiscriminator.DiscriminatorValue));
                var propertiesToWrite = allProperties
                    .Where(prop => prop != discriminatorProperty)
                    .Prepend(discriminatorProperty);
                foreach (var prop in propertiesToWrite)
                {
                    string propertyName = prop.Name == nameof(IPolymorphicDiscriminator.DiscriminatorValue)
                        ? DiscriminatorPropertyName
                        : prop.Name;
                    writer.WritePropertyName($"{char.ToLower(propertyName[0])}{propertyName.Substring(1)}");

                    var propValue = prop.GetValue(value);
                    JsonSerializer.Serialize(writer, propValue, options);
                }

                writer.WriteEndObject();
            }


            /// <inheritdoc />
            public override TBaseClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Parse the JSON document
                var jsonDoc = JsonDocument.ParseValue(ref reader);
                var jsonRoot = jsonDoc.RootElement;


                // Check for basic errors
                if (jsonRoot.ValueKind == JsonValueKind.Undefined)
                    throw new JsonException($@"Failed to parse an empty (or undefined) JSON into the target type ""{typeToConvert.Name}"".");
                if (jsonRoot.ValueKind == JsonValueKind.Null)
                    return null;
                if (jsonRoot.ValueKind != JsonValueKind.Object)
                    throw new JsonException($@"Failed to parse non-object JSON contents into the target type ""{typeToConvert.Name}"".");


                // Search for the discriminator field to know which type of object we're trying to parse
                if (jsonRoot.TryGetProperty(DiscriminatorPropertyName, out var discriminatorProp) == false)
                    throw new JsonException($@"Failed to parse polymorphic object instance due to missing ""{DiscriminatorPropertyName}"" property in the JSON contents.");

                var discriminatorValue = discriminatorProp.GetString();
                var cacheKey = new TypeDiscriminatorKey
                {
                    BaseType = typeof(TBaseClass),
                    DiscriminatorValue = discriminatorValue,
                };

                if (_typeDiscriminatorsCache.TryGetValue(cacheKey, out var targetType) == false)
                    throw new JsonException($@"Failed to parse polymorphic object instance: cannot determine the target instance type for discriminator ""{cacheKey.DiscriminatorValue}"" applied to the base type ""{cacheKey.BaseType.Name}"".");


                // Deserialize the object to the correct type
                var deserializedResult = (TBaseClass) JsonSerializer.Deserialize(jsonRoot.GetRawText(), targetType, options);
                return deserializedResult;
            }
        }
	}
}