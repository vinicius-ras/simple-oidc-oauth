using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Services;
using Xunit;

namespace SimpleOidcOauth.Tests.Unit.Services
{
    /// <summary>Tests for the <see cref="EmbeddedResourcesService" /> class.</summary>
    public class EmbeddedResourcesServiceTests
    {
        // CONSTANTS
        /// <summary>Namespace where the tests' embedded resources are to be located.</summary>
        private const string TEST_RESOURCES_NAMESPACE = "SimpleOidcOauth.EmbeddedResources.Tests";
        /// <summary>Path to the embedded resource used to test that the <see cref="EmbeddedResourcesService" /> can actually load resources.</summary>
        private const string TEST_VALID_RESOURCE_PATH = "ResourceLoadsTest.txt";
        /// <summary>Path to the embedded resource used to test that the <see cref="EmbeddedResourcesService" /> can actually load resources (nested version).</summary>
        private const string TEST_VALID_NESTED_RESOURCE_PATH = "Nested/Path/Nested-ResourceLoadsTest.txt";
        /// <summary>Path to the embedded resource used to test that the <see cref="EmbeddedResourcesService" /> fails to load invalid resources.</summary>
        private const string TEST_INVALID_RESOURCE_PATH = "INVALID-ResourceLoadsTest.txt";
        /// <summary>Path to the embedded resource used to test that the <see cref="EmbeddedResourcesService" /> fails to load invalid resources (nested version).</summary>
        private const string TEST_INVALID_NESTED_RESOURCE_PATH = "Nested/Path/INVALID-Nested-ResourceLoadsTest.txt";





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
            var resStream = resourceService.GetResourceStream(TEST_VALID_RESOURCE_PATH);

            Assert.True(resStream.Length > 0);
        }


        [Fact]
        public void GetResourceStream_ValidNestedResourcePath_ReturnsNonEmptyStream()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resStream = resourceService.GetResourceStream(TEST_VALID_NESTED_RESOURCE_PATH);

            Assert.True(resStream.Length > 0);
        }


        [Fact]
        public void GetResourceStream_NullResourcePath_ThrowsArgumentNullExcception()
        {
            var resourceService = CreateEmbeddedResourcesService();

            Assert.ThrowsAny<ArgumentNullException>(() => resourceService.GetResourceStream(null));
        }


        [Fact]
        public void GetResourceStream_InvalidResourcePath_ReturnsNull()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resStream = resourceService.GetResourceStream(TEST_INVALID_RESOURCE_PATH);

            Assert.Null(resStream);
        }


        [Fact]
        public void GetResourceStream_InvalidNestedResourcePath_ReturnsNull()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resStream = resourceService.GetResourceStream(TEST_INVALID_NESTED_RESOURCE_PATH);

            Assert.Null(resStream);
        }


        [Fact]
        public async Task GetResourceAsString_ValidResourcePath_ReturnsNonEmptyStream()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resString = await resourceService.GetResourceAsStringAsync(TEST_VALID_RESOURCE_PATH);

            Assert.True(resString.Length > 0);
        }


        [Fact]
        public async Task GetResourceAsString_NullResourcePath_ThrowsArgumentNullExcception()
        {
            var resourceService = CreateEmbeddedResourcesService();

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => resourceService.GetResourceAsStringAsync(null));
        }


        [Fact]
        public async Task GetResourceAsString_EmptyResourcePath_ReturnsNull()
        {
            var resourceService = CreateEmbeddedResourcesService();
            var resString = await resourceService.GetResourceAsStringAsync(TEST_INVALID_RESOURCE_PATH);

            Assert.Null(resString);
        }
    }
}
