namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>
	///     Models the configurations for the email sending service (<see cref="Services.IEmailService"/>).
	/// </summary>
	public class EmailConfigs
	{
		/// <summary>The host where the SMTP service used to send emails is running.</summary>
		/// <value>An IP or DNS Name for the host running the SMTP service.</value>
		public string Host { get; set; }
		/// <summary>The port where the SMTP service used to send emails is running on the target host.</summary>
		/// <value>A port number where the SMTP service is listening.</value>
		public int Port { get; set; }
		/// <summary>User name to be used to authenticate with the SMTP service.</summary>
		/// <value>A string representing the user name to be used when authenticating with the SMTP service.</value>
		public string UserName { get; set; }
		/// <summary>Password to be used to authenticate with the SMTP service.</summary>
		/// <value>A string representing the password to be used when authenticating with the SMTP service.</value>
		public string Password { get; set; }
		/// <summary>The default sender email's address to be used when sending an email.</summary>
		/// <value>A string representing the default "from" email's address to be used when sending an email.</value>
		public string DefaultSenderEmail { get; set; }
	}
}