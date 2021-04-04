using System.Collections.Generic;
using System.Linq;
using SimpleOidcOauth.Data.ValidationAttributes;
using Xunit;

namespace SimpleOidcOauth.Tests.Unit.Data.ValidationAttributes
{
	/// <summary>Tests for the <see cref="UrlOriginCollectionAttribute"/> validation attribute.</summary>
	public class UrlOriginCollectionAttributeTests
	{
		[Fact]
		public void IsValid_NullValue_ReturnsTrue()
		{
			IEnumerable<string> origins = null;
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.True(isValidResult);
		}


		[Fact]
		public void IsValid_EmptyStringsCollection_ReturnsTrue()
		{
			var origins = Enumerable.Empty<string>();
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.True(isValidResult);
		}


		[Fact]
		public void IsValid_NonStringsCollection_ReturnsFalse()
		{
			var origins = Enumerable.Empty<int>();
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_ValidOrigins_ReturnsTrue()
		{
			var origins = new string[] {
				"http://7d30e3ac699045dc94d927c8f59a483c.com",
				"https://bedf129725e9438fbc23aa39f782c566-144bc6bcb78b46e7b3efe7b8c7d8ade6.e261104534474bc59c8941d2ba7140fe.com",
			};
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.True(isValidResult);
		}


		[Fact]
		public void IsValid_SingleValidOrigin_ReturnsTrue()
		{
			var origins = new string[] { "https://a3be13b95a324696a96a6248bbd12ac4.com.br" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.True(isValidResult);
		}


		[Theory]
		[InlineData("abc-def")]
		[InlineData("ftp://40f6cd50e14247ffa309d9bb2695a58e.abc")]
		[InlineData("file://9916918ad5a04921b67be3498a6e0565.abc")]
		public void IsValid_SingleInvalidOrigin_ReturnsFalse(string origin)
		{
			var origins = new string[] { origin };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_OriginSpecifiedWithoutScheme_ReturnsFalse()
		{
			var origins = new string[] { "www.a6b05577a17c473cb583e587bd0e5bb6.com" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_OriginEndingWithSlash_ReturnsFalse()
		{
			var origins = new string[] { "https://www.c64dc2d740004ff5afbe7bcf1ba54c0b.com/" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_OriginContainingPathSegments_ReturnsFalse()
		{
			var origins = new string[] { "https://www.affba83d865c4e00be497298141a66ef.com/some/path/segment" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_OriginContainingQueryString_ReturnsFalse()
		{
			var origins = new string[] { "https://www.affba83d865c4e00be497298141a66ef.com?myVar=123" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_OriginContainingQueryStringWithoutVariables_ReturnsFalse()
		{
			var origins = new string[] { "https://www.a51f43f35380430eabad371af1a0a333.com?" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_OriginContainingEmptyFragment_ReturnsFalse()
		{
			var origins = new string[] { "https://www.a00d267ffe5746db9c5c6cc7f9171a19.com#" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_OriginContainingNonEmptyFragment_ReturnsFalse()
		{
			var origins = new string[] { "https://www.e46d6c58099c47178a793a881b4766db.com#my-fragment" };
			var attributeInstance = new UrlOriginCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(origins);

			Assert.False(isValidResult);
		}
	}
}