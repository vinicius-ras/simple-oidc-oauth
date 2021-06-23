namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>Implemented by classes which require polymorphism when being serialized/deserialized to/from JSON.</summary>
    /// <remarks>
    ///     <para>
    ///         The polymorphic convertion to/from JSON is implemented by the <see cref="PolymorphicDiscriminatorJsonConverterFactory"/> class via
    ///         the use of a discriminator property in the JSON payload.
    ///     </para>
    ///     <para>
    ///         Each concrete/leaf subclass in a given hierarchy should implement this interface and define a unique value for
    ///         the <see cref="IPolymorphicDiscriminator.DiscriminatorValue"/> property. That value will be used to represent
    ///         the concrete/leaf subclass in the JSON contents.
    ///     </para>
    ///     <para>
    ///         This discriminator value will be present in a property of the JSON contents. The name of this property is defined
    ///         by <see cref="PolymorphicDiscriminatorJsonConverterFactory.DiscriminatorPropertyName"/>.
    ///     </para>
    /// </remarks>
    public interface IPolymorphicDiscriminator
    {
        /// <summary>A discriminator value used to identify the implementing class in a JSON payload.</summary>
        /// <value>
        ///     This read-only property must contain a value to be used in the JSON contents and which will identify the specific
        ///     concrete/leaf type that implements the <see cref="IPolymorphicDiscriminator"/> interface.
        /// </value>
        string DiscriminatorValue { get; }
    }
}