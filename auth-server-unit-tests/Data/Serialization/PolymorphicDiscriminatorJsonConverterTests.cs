using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.Serialization;
using Xunit;

namespace SimpleOidcOauth.Tests.Unit.Data
{
	/// <summary>Unit tests for the <see cref="PolymorphicDiscriminatorJsonConverterFactory"/> class, along with its related <see cref="IPolymorphicDiscriminator"/> interface.</summary>
	public class PolymorphicDiscriminatorJsonConverterTests
	{
		// CONSTANTS
		/// <summary>Discriminator value for the first derived class type which was declared for the tests.</summary>
		private const string DerivedType1DiscriminatorValue = "first-derived-type";
		/// <summary>Discriminator value for the second derived class type which was declared for the tests.</summary>
		private const string DerivedType2DiscriminatorValue = "second-derived-type";
		/// <summary>A value used for a fixed/constant read-only property which will be used during the tests.</summary>
		private const string FixedReadOnlyPropValue = "random-value-45779b313e95453bbf94909ec8f445b8";
		/// <summary>A value used for a fixed/constant read-only property which will be used during the tests.</summary>
		private const int DerivedType2ReadOnlyPropValue = 918273645;
		/// <summary>An object to be used during the tests, declared as a base-type variable but instantiated as a first derived-type object.</summary>
		private static readonly BaseType _derivedObjectSample1 = new DerivedType1
		{
			StringProp = "StringProp-b57387b737cb47b5b0fabedaae2bc715",
			IntProp = 12345,
			InitReadOnlyProp = "init-prop-5d4c00512cb045d1a344d81a1c44fecc",
			DoubleArray = new [] { 1.2, 3.4, 5.6 },
			DerivedType1Prop = "DerivedType1Prop-36ef7f083c924f54abacf637c4bc13bb",
		};
		/// <summary>An object to be used during the tests, declared as a base-type variable but instantiated as a second derived-type object.</summary>
		private static readonly BaseType _derivedObjectSample2 = new DerivedType2
		{
			StringProp = "StringProp-0d9a01fde74b4f59b8a9c857eea43b2c",
			IntProp = 54321,
			InitReadOnlyProp = "init-prop-a0dcf5c1a21047dd80f3f80d286095fc",
			DoubleArray = new [] { 111.111, 222.222, 333.333 },
		};





		// NESTED TYPES
		/// <summary>A base class type used during the tests.</summary>
		private abstract class BaseType : IPolymorphicDiscriminator
		{
			public string StringProp { get; set; }
			public int IntProp { get; set; }
			public string FixedReadOnlyProp { get; } = FixedReadOnlyPropValue;
			public string InitReadOnlyProp { init; get; }
			public double[] DoubleArray { get; set; }

			public abstract string DiscriminatorValue { get; }
		}


		/// <summary>The first derived-class type used during the tests.</summary>
		private class DerivedType1 : BaseType
		{
			public string DerivedType1Prop { get; set; }

			public override string DiscriminatorValue => DerivedType1DiscriminatorValue;
		}


		/// <summary>The second derived-class type used during the tests.</summary>
		private class DerivedType2 : BaseType
		{
			public int DerivedType2ReadOnlyProp { get; } = DerivedType2ReadOnlyPropValue;
			public override string DiscriminatorValue => DerivedType2DiscriminatorValue;
		}





		// STATIC METHODS
		/// <summary>An utility method to roughly translate a property name from Pascal Case to "Camel Case".</summary>
		/// <remarks>This is a quick-and-dirty solution which simply converts the first character of whatever string is passed as input to lowercase.</remarks>
		/// <param name="propName">The Pascal-Cased property name.</param>
		/// <returns>Returns a Camel-Cased representation of the given property' name.</returns>
		private static string GetCamelCasePropertyName(string propName) => $"{char.ToLower(propName[0])}{propName.Substring(1)}";


		/// <summary>Verifies if two double values should be considered "equal" (for the purposes of the tests).</summary>
		/// <remarks>The equality verification simply verifies if the difference between the given numbers is less than an acceptable error value.</remarks>
		/// <param name="d1">The first double value to be compared for equality.</param>
		/// <param name="d2">The second double value to be compared for equality.</param>
		/// <returns>Returns a flag indicating if both values should be considered as "equal" (for the purposes of the tests).</returns>
		private static bool DoublesRoughlyEqual(double d1, double d2) => (Math.Abs(d1-d2) < 0.00001);


		/// <summary>Retrieves a <see cref="JsonSerializerOptions"/> configured for the integration tests.</summary>
		/// <returns>Returns a new instance of a pre-configured <see cref="JsonSerializerOptions"/> to be used in the tests.</returns>
		private static JsonSerializerOptions GetJsonSerializerOptionsForTests()
		{
			var converterConfigs = new PolymorphicDiscriminatorJsonConverterFactoryConfigs();
			converterConfigs.ExtraAssembliesToParse.Add(typeof(PolymorphicDiscriminatorJsonConverterTests).Assembly);

			var result = new JsonSerializerOptions(JsonSerializerDefaults.Web);
			result.Converters.Add(new PolymorphicDiscriminatorJsonConverterFactory(converterConfigs));

			return result;
		}




		// TESTS
		[Fact]
		public void Write_SerializingValidBaseTypeVariable_WritesJsonAsExpected()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();

			// Act
			var serializedDerived1 = JsonSerializer.Serialize(_derivedObjectSample1, serializationOptions);
			using var jsonDocObj1 = JsonDocument.Parse(serializedDerived1);

			var serializedDerived2 = JsonSerializer.Serialize(_derivedObjectSample2, serializationOptions);
			using var jsonDocObj2 = JsonDocument.Parse(serializedDerived2);


			// Assert
			Assert.Equal(_derivedObjectSample1.StringProp, jsonDocObj1.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.StringProp)))
				.GetString());
			Assert.Equal(_derivedObjectSample1.IntProp, jsonDocObj1.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.IntProp)))
				.GetInt32());
			Assert.Equal(_derivedObjectSample1.InitReadOnlyProp, jsonDocObj1.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.InitReadOnlyProp)))
				.GetString());
			Assert.Equal(_derivedObjectSample1.DoubleArray.Length, jsonDocObj1.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.DoubleArray)))
				.GetArrayLength());
			Assert.Equal(DerivedType1DiscriminatorValue, jsonDocObj1.RootElement
				.GetProperty(PolymorphicDiscriminatorJsonConverterFactory.DiscriminatorPropertyName)
				.GetString());
			Assert.Equal(
				((DerivedType1)_derivedObjectSample1).DerivedType1Prop,
				jsonDocObj1.RootElement
					.GetProperty(GetCamelCasePropertyName(nameof(DerivedType1.DerivedType1Prop)))
					.GetString());

			var arrayObj1 = jsonDocObj1.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.DoubleArray)));
			for (int i = _derivedObjectSample1.DoubleArray.Length - 1; i >= 0; i-- )
				Assert.True(DoublesRoughlyEqual(_derivedObjectSample1.DoubleArray[i], arrayObj1[i].GetDouble()));

			Assert.Equal(_derivedObjectSample2.StringProp, jsonDocObj2.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.StringProp)))
				.GetString());
			Assert.Equal(_derivedObjectSample2.IntProp, jsonDocObj2.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.IntProp)))
				.GetInt32());
			Assert.Equal(_derivedObjectSample2.InitReadOnlyProp, jsonDocObj2.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.InitReadOnlyProp)))
				.GetString());
			Assert.Equal(DerivedType2DiscriminatorValue, jsonDocObj2.RootElement
				.GetProperty(PolymorphicDiscriminatorJsonConverterFactory.DiscriminatorPropertyName)
				.GetString());
			Assert.Equal(
				((DerivedType2)_derivedObjectSample2).DerivedType2ReadOnlyProp,
				jsonDocObj2.RootElement
					.GetProperty(GetCamelCasePropertyName(nameof(DerivedType2.DerivedType2ReadOnlyProp)))
					.GetInt32());

			var arrayObj2 = jsonDocObj2.RootElement
				.GetProperty(GetCamelCasePropertyName(nameof(BaseType.DoubleArray)));
			for (int i = _derivedObjectSample2.DoubleArray.Length - 1; i >= 0; i-- )
				Assert.True(DoublesRoughlyEqual(_derivedObjectSample2.DoubleArray[i], arrayObj2[i].GetDouble()));
		}


		[Fact]
		public void WriteAndRead_DeserializingCorrectlySerializedData_ReadsObjectsAsExpected()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();

			// Act
			var serializedDerived1 = JsonSerializer.Serialize(_derivedObjectSample1, serializationOptions);
			var parsedDerived1 = JsonSerializer.Deserialize<BaseType>(serializedDerived1, serializationOptions);

			var serializedDerived2 = JsonSerializer.Serialize(_derivedObjectSample2, serializationOptions);
			var parsedDerived2 = JsonSerializer.Deserialize<BaseType>(serializedDerived2, serializationOptions);

			// Assert
			Assert.IsType<DerivedType1>(parsedDerived1);
			Assert.IsType<DerivedType2>(parsedDerived2);

			Assert.Equal(_derivedObjectSample1.StringProp, parsedDerived1.StringProp);
			Assert.Equal(_derivedObjectSample1.IntProp, parsedDerived1.IntProp);
			Assert.Equal(_derivedObjectSample1.InitReadOnlyProp, parsedDerived1.InitReadOnlyProp);
			Assert.Equal(_derivedObjectSample1.DiscriminatorValue, parsedDerived1.DiscriminatorValue);
			Assert.Equal(DerivedType1DiscriminatorValue, parsedDerived1.DiscriminatorValue);
			Assert.Equal(((DerivedType1)_derivedObjectSample1).DerivedType1Prop, ((DerivedType1)parsedDerived1).DerivedType1Prop);
			Assert.Equal(_derivedObjectSample1.DoubleArray.Length, parsedDerived1.DoubleArray.Length);
			for (int i = _derivedObjectSample1.DoubleArray.Length - 1; i >= 0; i--)
				Assert.True(DoublesRoughlyEqual(_derivedObjectSample1.DoubleArray[i], parsedDerived1.DoubleArray[i]));

			Assert.Equal(_derivedObjectSample2.StringProp, parsedDerived2.StringProp);
			Assert.Equal(_derivedObjectSample2.IntProp, parsedDerived2.IntProp);
			Assert.Equal(_derivedObjectSample2.InitReadOnlyProp, parsedDerived2.InitReadOnlyProp);
			Assert.Equal(_derivedObjectSample2.DiscriminatorValue, parsedDerived2.DiscriminatorValue);
			Assert.Equal(DerivedType2DiscriminatorValue, parsedDerived2.DiscriminatorValue);
			Assert.Equal(((DerivedType2)_derivedObjectSample2).DerivedType2ReadOnlyProp, ((DerivedType2)parsedDerived2).DerivedType2ReadOnlyProp);
			Assert.Equal(_derivedObjectSample2.DoubleArray.Length, parsedDerived2.DoubleArray.Length);
			for (int i = _derivedObjectSample2.DoubleArray.Length - 1; i >= 0; i--)
				Assert.True(DoublesRoughlyEqual(_derivedObjectSample2.DoubleArray[i], parsedDerived2.DoubleArray[i]));
		}


		[Fact]
		public void WriteAndRead_DeserializingEmptyJson_ThrowsJsonException()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();
			const string jsonContents = @"";

			// Act
			Action deserializationAction = () => JsonSerializer.Deserialize<BaseType>(jsonContents, serializationOptions);


			// Assert
			Assert.Throws<JsonException>(deserializationAction);
		}


		[Fact]
		public void WriteAndRead_DeserializingNumericJsonValue_ThrowsJsonException()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();
			const string jsonContents1 = @"10",
				jsonContents2 = @"15.27";

			// Act
			Action deserializationAction1 = () => JsonSerializer.Deserialize<BaseType>(jsonContents1, serializationOptions),
				deserializationAction2 = () => JsonSerializer.Deserialize<BaseType>(jsonContents2, serializationOptions);


			// Assert
			Assert.Throws<JsonException>(deserializationAction1);
			Assert.Throws<JsonException>(deserializationAction2);
		}


		[Fact]
		public void WriteAndRead_DeserializingBooleanJsonValue_ThrowsJsonException()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();
			const string jsonContents1 = @"true",
				jsonContents2 = @"false";

			// Act
			Action deserializationAction1 = () => JsonSerializer.Deserialize<BaseType>(jsonContents1, serializationOptions),
				deserializationAction2 = () => JsonSerializer.Deserialize<BaseType>(jsonContents2, serializationOptions);


			// Assert
			Assert.Throws<JsonException>(deserializationAction1);
			Assert.Throws<JsonException>(deserializationAction2);
		}


		[Fact]
		public void WriteAndRead_DeserializingStringJsonValue_ThrowsJsonException()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();
			const string randomStringJsonValue = @"""some-string-8089d905b8914879b83dc30556cb06ed""",
				emptyStringJsonValue = @"""""";

			// Act
			Action deserializationAction1 = () => JsonSerializer.Deserialize<BaseType>(randomStringJsonValue, serializationOptions),
				deserializationAction2 = () => JsonSerializer.Deserialize<BaseType>(emptyStringJsonValue, serializationOptions);


			// Assert
			Assert.Throws<JsonException>(deserializationAction1);
			Assert.Throws<JsonException>(deserializationAction2);
		}


		[Fact]
		public void WriteAndRead_DeserializingArrayJsonValue_ThrowsJsonException()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();
			const string jsonContents1 = @"[1, 2, 3]",
				jsonContents2 = @"[]",
				jsonContents3 = @"[""abc"", ""def""]";

			// Act
			Action deserializationAction1 = () => JsonSerializer.Deserialize<BaseType>(jsonContents1, serializationOptions),
				deserializationAction2 = () => JsonSerializer.Deserialize<BaseType>(jsonContents2, serializationOptions),
				deserializationAction3 = () => JsonSerializer.Deserialize<BaseType>(jsonContents3, serializationOptions);


			// Assert
			Assert.Throws<JsonException>(deserializationAction1);
			Assert.Throws<JsonException>(deserializationAction2);
			Assert.Throws<JsonException>(deserializationAction3);
		}


		[Fact]
		public void WriteAndRead_DeserializingNullJsonValue_ReturnsNullResult()
		{
			// Arrange
			var serializationOptions = GetJsonSerializerOptionsForTests();
			const string jsonContents = @"null";

			// Act
			var parsedObject = JsonSerializer.Deserialize<BaseType>(jsonContents, serializationOptions);


			// Assert
			Assert.Null(parsedObject);
		}


		[Fact]
		public async Task WriteAndRead_DeserializingValidJsonContentsWithoutDiscriminator_ThrowsJsonException()
		{
			// ***** Arrange *****
			var serializationOptions = GetJsonSerializerOptionsForTests();



			// ***** Act *****
			var validSerializedJsonContents = JsonSerializer.Serialize(_derivedObjectSample1, serializationOptions);
			using var validJsonDoc = JsonDocument.Parse(validSerializedJsonContents);

			// Rewrite all of the JSON contents, but removing the "discriminator property" from the original contents
			var allNonDiscriminatorJsonProperties = validJsonDoc.RootElement
				.EnumerateObject()
				.Where(jsonProperty => jsonProperty.Name != PolymorphicDiscriminatorJsonConverterFactory.DiscriminatorPropertyName);

			string jsonContentsWithoutDiscriminator = null;
			using (var memStream = new MemoryStream())
			{
				using (var jsonWriter = new Utf8JsonWriter(memStream))
				{
					jsonWriter.WriteStartObject();
					foreach (var property in allNonDiscriminatorJsonProperties)
						property.WriteTo(jsonWriter);
					jsonWriter.WriteEndObject();
				}

				memStream.Seek(0, SeekOrigin.Begin);
				using (var streamReader = new StreamReader(memStream))
				{
					jsonContentsWithoutDiscriminator = await streamReader.ReadToEndAsync();
				}
			}

			// This action would try to parse the new JSON version (which doesn't contain the "discriminator property" anymore)
			Action deserializationAction = () => JsonSerializer.Deserialize<BaseType>(jsonContentsWithoutDiscriminator, serializationOptions);



			// ***** Assert *****
			Assert.Throws<JsonException>(deserializationAction);
		}
	}
}