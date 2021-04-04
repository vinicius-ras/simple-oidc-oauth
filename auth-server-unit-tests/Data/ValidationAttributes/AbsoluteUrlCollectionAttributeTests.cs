using System.Collections.Generic;
using System.Linq;
using SimpleOidcOauth.Data.ValidationAttributes;
using Xunit;

namespace SimpleOidcOauth.Tests.Unit.Data.ValidationAttributes
{
	/// <summary>Tests for the <see cref="AbsoluteUrlCollectionAttribute"/> validation attribute.</summary>
	public class AbsoluteUrlCollectionAttributeTests
	{
		[Fact]
		public void IsValid_NullValue_ReturnsTrue()
		{
			IEnumerable<string> urls = null;
			var attributeInstance = new AbsoluteUrlCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(urls);

			Assert.True(isValidResult);
		}


		[Fact]
		public void IsValid_EmptyStringsCollection_ReturnsTrue()
		{
			var urls = Enumerable.Empty<string>();
			var attributeInstance = new AbsoluteUrlCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(urls);

			Assert.True(isValidResult);
		}


		[Fact]
		public void IsValid_NonStringsCollection_ReturnsFalse()
		{
			var nonStringsCollection = Enumerable.Empty<int>();
			var attributeInstance = new AbsoluteUrlCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(nonStringsCollection);

			Assert.False(isValidResult);
		}


		[Fact]
		public void IsValid_ValidAbsoluteUrls_ReturnsTrue()
		{
			var absoluteUrls = new string[] {
				"http://ecd9fc21b7a3487ea6addc7e8a502aa3.com",
				"http://28a111b03c774fbfb48f470041663eb7.com/",
				"http://www.75d729a385fd43d1bfcbeaa1b6b4c9a3.com",
				"http://www.5533a85eccd94693a236e454cd1d9608.com/",
				"https://e7b8a807688a47619a5095536187b30a-c4b5cca9b4054182a6cd9fd4738f4803.7ec8a3db4a8941b2836ac7a57d8b19bf.com/some/path",
				"https://baa4a885d9794c39afd0f0902dd328f5-6447bb3e31e44c4ca56531880b89bc8a.847b0b130a5741e7a4da6b1a247168f8.com/some/path?with-query=yes",
				"https://e4e6dd8ba00d47f2a21741f521bbedc0-293a6dbf22d6411ea334812ab9c0245d.aeb727af826641c297f61b865f01381c.com/some/path?with-query=yes#with-fragment",
				"https://b9558411b6ab4882a892010bed590aee-d96fb1caad4a4626bd5d09587c5cb951.6553f24847dd435b88d25193b77fc4df.com/some/path#with-fragment",
				"https://b2bdd3c97e1845cfb6ec0872520a1aa0-521c8fa123e143afbdc1890037f5670f.f315fb16e37547a4b3bc83656f9ad354.com?with-query=yes",
				"https://df5846e85f8d45b88657dbe484703ff1-d109c0ede30a40bbb7bddcad2715bc8c.e417c2d12b94435f93e789523d328b59.com#with-fragment",
				"https://b2bdd3c97e1845cfb6ec0872520a1aa0-521c8fa123e143afbdc1890037f5670f.f315fb16e37547a4b3bc83656f9ad354.com?with-query=yes#with-fragment",
				"https://empty-query-string.0ff1c19c477046d4abc1cc5bd048cf9c.com?",
				"https://empty-framgent.0ff1c19c477046d4abc1cc5bd048cf9c.com#",
				"https://empty-query-string-and-framgent.0ff1c19c477046d4abc1cc5bd048cf9c.com?#",
			};
			var attributeInstance = new AbsoluteUrlCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(absoluteUrls);

			Assert.True(isValidResult);
		}


		[Fact]
		public void IsValid_SingleValidAbsoluteUrl_ReturnsTrue()
		{
			var absoluteUrls = new string[] { "https://b68bbd56f4714a3384b9c7c1d0e0d4b3.com.br/abc?def=ghi#jkl" };
			var attributeInstance = new AbsoluteUrlCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(absoluteUrls);

			Assert.True(isValidResult);
		}


		[Theory]
		[InlineData("/")]
		[InlineData("abc-def")]
		[InlineData("/some/relative/path")]
		[InlineData("some/other/relative/path")]
		[InlineData("?some-query=123&not-acceptable=true")]
		[InlineData("#fragment-name")]
		[InlineData("?some-query=123&not-acceptable=true#fragment-name")]
		[InlineData("/some/relative/path?some-query=123&not-acceptable=true#fragment-name")]
		[InlineData("www.domain-only-15d9e1fc561e4d4dadaff89e40a8eb4f.com")]
		[InlineData("www.domain-only-15d9e1fc561e4d4dadaff89e40a8eb4f.com/")]
		[InlineData("www.domain-only-15d9e1fc561e4d4dadaff89e40a8eb4f.com/some/path")]
		[InlineData("ftp://invalid-scheme-40f6cd50e14247ffa309d9bb2695a58e.abc")]
		public void IsValid_SingleInvalidAbsoluteUrl_ReturnsFalse(string url)
		{
			var urls = new string[] { url };
			var attributeInstance = new AbsoluteUrlCollectionAttribute();

			bool isValidResult = attributeInstance.IsValid(urls);

			Assert.False(isValidResult);
		}
	}
}