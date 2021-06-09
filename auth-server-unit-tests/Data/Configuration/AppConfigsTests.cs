using System;
using SimpleOidcOauth.Data.Configuration;
using Xunit;

namespace SimpleOidcOauth.Tests.Unit.Data.Configuration
{
	/// <summary>Tests for the <see cref="AppConfigs"/> class.</summary>
	public class AppConfigsTests
	{
		// TESTS
		[Fact]
		public void GetAppConfigurationKey_GetRootObjectOnly_ThrowsArgumentException()
		{
			Action action = () => AppConfigs.GetAppConfigurationKey(configs => configs);
			Assert.Throws<ArgumentException>(action);
		}


		[Fact]
		public void GetAppConfigurationKey_GetSpaBaseUrl_ReturnsCorrectKey()
		{
			string configKey = AppConfigs.GetAppConfigurationKey(configs => configs.Spa.BaseUrl);
			Assert.Equal($"{AppConfigs.ConfigKey}:{nameof(AppConfigs.Spa)}:{nameof(AppConfigs.Spa.BaseUrl)}", configKey);
		}


		[Fact]
		public void GetAppConfigurationKey_GetEmbeddedResourcesNamespace_ReturnsCorrectKey()
		{
			string configKey = AppConfigs.GetAppConfigurationKey(configs => configs.EmbeddedResources.ResourcesNamespace);
			Assert.Equal($"{AppConfigs.ConfigKey}:{nameof(AppConfigs.EmbeddedResources)}:{nameof(AppConfigs.EmbeddedResources.ResourcesNamespace)}", configKey);
		}
	}
}