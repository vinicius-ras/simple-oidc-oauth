using SimpleOidcOauth.Controllers;

namespace SimpleOidcOauth.Data
{
	/// <summary>Class containing information about the application's endpoints.</summary>
	public static class AppEndpoints
	{
		/// <summary>The prefix used for all web-API endpoints.</summary>
		public const string ApiUriPrefix = "/api";





		#region ENDPOINTS: AccountController
		/// <summary>The base URI for the <see cref="AccountController"/> action endpoints.</summary>
		public const string AccountControllerUri = ApiUriPrefix + "/account";
		/// <summary>URI for the <see cref="AccountController.Login(Models.LoginInputModel)"/> action endpoint.</summary>
		public const string LoginUri = AccountControllerUri + "/login";
		/// <summary>URI for the <see cref="AccountController.Logout(string)"/> action endpoint.</summary>
		public const string LogoutUri = AccountControllerUri + "/logout";
		/// <summary>URI for the <see cref="AccountController.CheckLogin"/> action endpoint.</summary>
		public const string CheckLoginUri = AccountControllerUri + "/check-login";
		/// <summary>URI for the <see cref="AccountController.Register(Models.AccountRegisterInputModel)"/> action endpoint.</summary>
		public const string RegisterUri = AccountControllerUri;
		/// <summary>URI for the <see cref="AccountController.VerifyAccount(string, string)"/> action endpoint.</summary>
		public const string VerifyAccountUri = AccountControllerUri + "/verify-account";
		#endregion





		#region ENDPOINTS: ClientsManagementController
		/// <summary>The base URI for the <see cref="ClientsManagementController"/> action endpoints.</summary>
		public const string ClientsManagementControllerUri = ApiUriPrefix + "/management/clients";
		/// <summary>URI for the <see cref="ClientsManagementController.GetAllClients"/> action endpoint.</summary>
		public const string GetAllRegisteredClients = ClientsManagementControllerUri;
		/// <summary>URI for the <see cref="ClientsManagementController.GetClient(string)"/> action endpoint.</summary>
		public const string GetRegisteredClient = ClientsManagementControllerUri + "/{" + ClientIdParameterName + "}";
		/// <summary>URI for the <see cref="ClientsManagementController.GetAllowedClientRegistrationGrantTypes"/> action endpoint.</summary>
		public const string GetAllowedClientRegistrationGrantTypes = ClientsManagementControllerUri + "/allowed-grant-types";
		/// <summary>URI for the <see cref="ClientsManagementController.GetAvailableResources"/> action endpoint.</summary>
		public const string GetAvailableClientRegistrationResources = ClientsManagementControllerUri + "/available-resources";
		/// <summary>URI for the <see cref="ClientsManagementController.CreateNewClientApplication(Serialization.SerializableClient)"/> action endpoint.</summary>
		public const string CreateNewClientApplication = ClientsManagementControllerUri;
		/// <summary>URI for the <see cref="ClientsManagementController.UpdateClientApplication(string, Serialization.SerializableClient)"/> action endpoint.</summary>
		public const string UpdateClientApplication = ClientsManagementControllerUri + "/{" + ClientIdParameterName + "}";

		/// <summary>The name of the URI parameter used for specifying Client IDs in the <see cref="ClientsManagementController"/> endpoints.</summary>
		public const string ClientIdParameterName = "clientID";
		#endregion





		#region ENDPOINTS: IdentityServerErrorsController
		/// <summary>The base URI for the <see cref="IdentityServerErrorsController"/> action endpoints.</summary>
		public const string IdentityServerErrorsControllerUri = ApiUriPrefix + "/idp-error";
		/// <summary>URI for the <see cref="IdentityServerErrorsController.Error(string)"/> action endpoint.</summary>
		public const string IdpErrorUri = IdentityServerErrorsControllerUri;
		#endregion





		#region ENDPOINTS: UnhandledExceptionsController
		/// <summary>URI for the <see cref="UnhandledExceptionsController.OnUnhandledException"/> action endpoint.</summary>
		public const string UnhandledExceptionUri = "/unhandled-exception";
		#endregion
	}
}