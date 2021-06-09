using System;
using SimpleOidcOauth.Utilities;
using Xunit;

namespace SimpleOidcOauth.Tests.Unit.Utilities
{
	/// <summary>Tests for the <see cref="ExpressionTreeUtilities"/> class.</summary>
	public class ExpressionTreeUtilitiesTests
	{
		// NESTED CLASSES
		#region Classes used for the tests
		#pragma warning disable CS0649
		private class FirstTestClass
		{
			public string FirstProperty { get; set; }
			public int SecondProperty { get; set; }
			public SecondTestClass ComplexObjectProperty { get; set; }
			public float firstField;
			public object secondField;
			public SecondTestClass complexObjectField;

			public string FirstMethod() => "testing";
			public SecondTestClass SecondMethod() => default;
		}


		private class SecondTestClass
		{
			public string NestedReadOnlyProperty { get; }
			public double NestedFieldBackedProperty {
				get => _nestedFieldBackedPropertyField;
				set
				{
					_nestedFieldBackedPropertyField = value;
				}
			}
			public float nestedField;

			private double _nestedFieldBackedPropertyField;
		}
		#pragma warning restore CS0649
		#endregion





		// TESTS
		[Fact]
		public void GetMemberAccessPath_NullObjectAccessingProperty_ReturnsCorrectPath() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			string result = ExpressionTreeUtilities.GetMemberAccessPath(obj, obj => obj.FirstProperty, "|", false);

			// Assert
			Assert.Equal("obj|FirstProperty", result);
		}


		[Fact]
		public void GetMemberAccessPath_NullObjectAccessingField_ReturnsCorrectPath() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			string result = ExpressionTreeUtilities.GetMemberAccessPath(obj, obj => obj.firstField, "/", false);

			// Assert
			Assert.Equal("obj/firstField", result);
		}


		[Fact]
		public void GetMemberAccessPath_NotNullObjectAccessingProperty_ReturnsCorrectPath() {
			// Arrange
			FirstTestClass obj = new FirstTestClass();

			// Act
			string result = ExpressionTreeUtilities.GetMemberAccessPath(obj, myObj => myObj.SecondProperty, "***", false);

			// Assert
			Assert.Equal("myObj***SecondProperty", result);
		}


		[Fact]
		public void GetMemberAccessPath_NotNullObjectAccessingField_ReturnsCorrectPath() {
			// Arrange
			FirstTestClass obj = new FirstTestClass();

			// Act
			string result = ExpressionTreeUtilities.GetMemberAccessPath(obj, someObj => someObj.secondField, "<===>", false);

			// Assert
			Assert.Equal("someObj<===>secondField", result);
		}


		[Fact]
		public void GetMemberAccessPath_AccessingNestedReadOnlyProperty_ReturnsCorrectPath() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			string result = ExpressionTreeUtilities.GetMemberAccessPath(obj, obj => obj.ComplexObjectProperty.NestedReadOnlyProperty, ":", false);

			// Assert
			Assert.Equal("obj:ComplexObjectProperty:NestedReadOnlyProperty", result);
		}


		[Fact]
		public void GetMemberAccessPath_AccessingNestedFieldBackedProperty_ReturnsCorrectPath() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			string result = ExpressionTreeUtilities.GetMemberAccessPath(obj, obj => obj.ComplexObjectProperty.NestedFieldBackedProperty, ":", false);

			// Assert
			Assert.Equal("obj:ComplexObjectProperty:NestedFieldBackedProperty", result);
		}


		[Fact]
		public void GetMemberAccessPath_AccessingNestedField_ReturnsCorrectPath() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			string result = ExpressionTreeUtilities.GetMemberAccessPath(obj, randomObj => randomObj.ComplexObjectProperty.nestedField, "-+-", false);

			// Assert
			Assert.Equal("randomObj-+-ComplexObjectProperty-+-nestedField", result);
		}


		[Fact]
		public void GetMemberAccessPath_SupressRootObject_ReturnsPathWithoutFirstComponent() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			string resultWithRoot = ExpressionTreeUtilities.GetMemberAccessPath(obj, anotherObj => anotherObj.ComplexObjectProperty.nestedField, "_", false),
				resultWithoutRoot = ExpressionTreeUtilities.GetMemberAccessPath(obj, anotherObj => anotherObj.ComplexObjectProperty.nestedField, "_", true);

			// Assert
			Assert.Equal("anotherObj_ComplexObjectProperty_nestedField", resultWithRoot);
			Assert.Equal("ComplexObjectProperty_nestedField", resultWithoutRoot);
		}


		[Fact]
		public void GetMemberAccessPath_AccessingMethod_ThrowsArgumentException() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			Action action = () => ExpressionTreeUtilities.GetMemberAccessPath(obj, obj => obj.FirstMethod(), ":", false);

			// Assert
			Assert.Throws<ArgumentException>(action);
		}


		[Fact]
		public void GetMemberAccessPath_AccessingMethodReturnObject_ThrowsArgumentException() {
			// Arrange
			FirstTestClass obj = null;

			// Act
			Action action = () => ExpressionTreeUtilities.GetMemberAccessPath(obj, obj => obj.SecondMethod().nestedField, ":", false);

			// Assert
			Assert.Throws<ArgumentException>(action);
		}
	}
}