using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleOidcOauth.SampleClients.Mvc.Data.Configuration;

namespace SimpleOidcOauth.SampleClients.Mvc.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index() => View();

		[Authorize]
		public IActionResult Secret() => View();

		public IActionResult Logout() => SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectConfigs.AuthenticationSchemeName);
	}
}