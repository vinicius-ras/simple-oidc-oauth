using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Services;
using Xunit;

namespace SimpleOidcOauth.Tests
{
    public class EmbeddedResourcesServiceTests
    {
        // CONSTANTS
        /// <summary>Namespace where the tests' embedded resources are to be located.</summary>
        private const string TEST_RESOURCES_NAMESPACE = "SimpleOidcOauth.EmbeddedResources.Tests";
        /// <summary>Path to the embedded resource used to test that the <see cref="EmbeddedResourcesService" /> can actually load resources.</summary>
        private const string TEST_RESOURCE_ID_EMAIL_TEMPLATE_LOADS_TEST = "ResourceLoadsTest.txt";
        /// <summary>Path to the embedded resource used to test that the <see cref="EmbeddedResourcesService" /> fails to load invalid resources.</summary>
        private const string TEST_RESOURCE_ID_EMAIL_TEMPLATE_LOADS_TEST_INVALID = "INVALID-ResourceLoadsTest.txt";





        // INSTANCE METHODS
        /// <summary>Instantiates a new <see cref="IEmbeddedResourcesService"/> for testing purposes.</summary>
        /// <returns>Returns the newly created <see cref="IEmbeddedResourcesService"/>, which might then be used for the tests.</returns>
        private IEmbeddedResourcesService CreateEmbeddedResourcesService()
        {
            var appConfigsOptions = Options.Create(new AppConfigs {
                EmbeddedResources = new EmbeddedResourcesConfigs {
                    ResourcesNamespace = TEST_RESOURCES_NAMESPACE
                }
            });

            return new EmbeddedResourcesService(appConfigsOptions);
        }





        // TESTS
        [Fact]
        public void GetResourceStream_ValidResourcePath_ReturnsNonEmptyStream()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resStream = resourceService.GetResourceStream(TEST_RESOURCE_ID_EMAIL_TEMPLATE_LOADS_TEST);

            Assert.True(resStream.Length > 0);
        }


        [Fact]
        public void GetResourceStream_NullResourcePath_ThrowsArgumentNullExcception()
        {
            var resourceService = CreateEmbeddedResourcesService();

            Assert.ThrowsAny<ArgumentNullException>(() => resourceService.GetResourceStream(null));
        }


        [Fact]
        public void GetResourceStream_EmptyResourcePath_ReturnsNull()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resStream = resourceService.GetResourceStream(TEST_RESOURCE_ID_EMAIL_TEMPLATE_LOADS_TEST_INVALID);

            Assert.Null(resStream);
        }


        [Fact]
        public async Task GetResourceAsString_ValidResourcePath_ReturnsNonEmptyStream()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resString = await resourceService.GetResourceAsString(TEST_RESOURCE_ID_EMAIL_TEMPLATE_LOADS_TEST);

            Assert.True(resString.Length > 0);
        }


        [Fact]
        public async Task GetResourceAsString_NullResourcePath_ThrowsArgumentNullExcception()
        {
            var resourceService = CreateEmbeddedResourcesService();

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => resourceService.GetResourceAsString(null));
        }


        [Fact]
        public async Task GetResourceAsString_EmptyResourcePath_ReturnsNull()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resString = await resourceService.GetResourceAsString(TEST_RESOURCE_ID_EMAIL_TEMPLATE_LOADS_TEST_INVALID);

            Assert.Null(resString);
        }
    }
}
