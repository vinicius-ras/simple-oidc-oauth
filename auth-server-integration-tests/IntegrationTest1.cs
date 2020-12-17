using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SimpleOidcOauth.Tests.Integration
{
    /// <summary>This is a simple placeholder integration suite of tests, which will be removed in future revisions.</summary>
    public class IntegrationTest1 : IClassFixture<WebApplicationFactory<Startup>>
    {
        // FIELDS
		private readonly WebApplicationFactory<Startup> _webAppFactory;





		// INSTANCE METHODS
		public IntegrationTest1(WebApplicationFactory<Startup> webAppFactory)
        {
            _webAppFactory = webAppFactory;
        }





        // TESTS
        [Fact]
        public async Task Test1()
        {
            var httpClient = _webAppFactory.CreateClient();

            HttpResponseMessage responseInvalidRoute = await httpClient.GetAsync("/not-existant"),
                responseUnauthorizedRoute = await httpClient.GetAsync("/api/Account/check-login");

            Assert.Equal(HttpStatusCode.NotFound, responseInvalidRoute.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, responseUnauthorizedRoute.StatusCode);
        }
    }
}
