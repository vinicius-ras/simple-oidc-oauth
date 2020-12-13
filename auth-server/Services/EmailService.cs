using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using SimpleOidcOauth.Data.Configuration;

namespace SimpleOidcOauth.Services
{
	/// <summary>Implementation for the <see cref="IEmailService" />.</summary>
	public class EmailService : IEmailService
	{
		// FIELDS
		/// <summary>Container-injected email configurations.</summary>
		private readonly EmailConfigs _emailConfigs;
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</summary>
		private readonly ILogger<EmailService> _logger;
		/// <summary>Container-injected instance for the <see cref="IEmbeddedResourcesService" /> service.</summary>
		private readonly IEmbeddedResourcesService _embeddedResourcesService;
		/// <summary>Container-injected instance of an <see cref="ISmtpClient" />.</summary>
		private readonly ISmtpClient _smtpClient;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="appConfigs">Container-injected application configurations.</param>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		/// <param name="embeddedResourcesService">Container-injected instance for the <see cref="IEmbeddedResourcesService" /> service.</param>
		/// <param name="smtpClient">Container-injected instance of an <see cref="ISmtpClient" />.</param>
		public EmailService(IOptions<AppConfigs> appConfigs, ILogger<EmailService> logger, IEmbeddedResourcesService embeddedResourcesService, ISmtpClient smtpClient)
		{
			_emailConfigs = appConfigs.Value.Email;
			_logger = logger;
			_embeddedResourcesService = embeddedResourcesService;
			_smtpClient = smtpClient;
		}


		/// <summary>The actual implementation which is used to send the email messages through an SMTP client.</summary>
		/// <param name="msg">An object representing the message to be sent.</param>
		/// <param name="cancellationToken">An optional cancellation token, which can be used to cancel the email sending operation,</param>
		/// <returns>
		///     Returns an asynchronous task, which resolves into a flag that indicates if the sending operation was successful.
		///     Notice: due to security restrictions in some SMTP servers, this method might return a <c>true</c> flag even if the email was sent to
		///     an inexistant recipient.
		/// </returns>
		private async Task<bool> SendMessageAsync(MimeMessage msg, CancellationToken cancellationToken = default(CancellationToken))
		{
			bool emailConnected = false,
				emailSent = false;
			try
			{
				// Try to connect to SMTP server, authenticate (if necessary), send the email, and disconnect from the server
				await _smtpClient.ConnectAsync(_emailConfigs.Host, _emailConfigs.Port, cancellationToken: cancellationToken);
				emailConnected = true;

				if (string.IsNullOrWhiteSpace(_emailConfigs.UserName) == false || string.IsNullOrWhiteSpace(_emailConfigs.Password) == false)
					await _smtpClient.AuthenticateAsync(_emailConfigs.UserName, _emailConfigs.Password, cancellationToken: cancellationToken);
				await _smtpClient.SendAsync(msg, cancellationToken: cancellationToken);
				emailSent = true;
			}
			catch (OperationCanceledException) {
				// This exception will be rethrown
				throw;
			}
			catch (Exception ex) when (
				ex is SystemException
				|| ex is SslHandshakeException
				|| ex is SmtpCommandException
				|| ex is SmtpProtocolException
			)
			{
				_logger.LogError(ex, "Failed to send email message.");
			}
			finally
			{
				if (emailConnected)
				{
					try
					{
						await _smtpClient.DisconnectAsync(true, cancellationToken: cancellationToken);
					}
					catch (Exception ex) when (
						ex is ObjectDisposedException
						|| ex is ServiceNotConnectedException
						|| ex is OperationCanceledException
						|| ex is IOException
						|| ex is CommandException
						|| ex is ProtocolException
					)
					{
						// Upon disconnection failue, log either an
						Action<Exception, string, object[]> logMethod;
						if (emailSent)
							logMethod = _logger.LogWarning;
						else
							logMethod = _logger.LogError;
						logMethod(ex, $"Failed to disconnect from SMTP server.", null);
					}
				}
			}
			return emailSent;
		}





		// INTERFACE IMPLEMENTATION: IEmailService
		/// <inheritdoc/>
		public async Task<bool> SendMessageFromStringAsync(string to, string subject, string htmlBodyTemplate, IDictionary<string, object> emailData, CancellationToken cancellationToken)
		{
			// Error checking: check for null arguments
			if (to == null)
				throw new ArgumentNullException(nameof(to));
			if (subject == null)
				throw new ArgumentNullException(nameof(subject));
			if (htmlBodyTemplate == null)
				throw new ArgumentNullException(nameof(htmlBodyTemplate));


			// Render email's body by using the template and given data
			emailData = emailData ?? ImmutableDictionary<string, object>.Empty;
			string renderedEmail = Regex.Replace(htmlBodyTemplate, @"\$\{([^\d][_\w]+[_\w\d]*)\}", match =>
			{
				if (emailData.TryGetValue(match.Groups[1].Value, out object dataValue))
					return dataValue.ToString();
				return match.Value;
			});


			// Build the email message and send it
			var msg = new MimeMessage();
			try
			{
				msg.From.Add(new MailboxAddress(_emailConfigs.DefaultSenderEmail, _emailConfigs.DefaultSenderEmail));
			}
			catch (ParseException ex)
			{
				_logger.LogError(ex, $@"Failed to send email using malformed SENDER email ""{_emailConfigs.DefaultSenderEmail}""");
				return false;
			}

			try
			{
				msg.To.Add(new MailboxAddress(to, to));
			}
			catch (ParseException ex)
			{
				_logger.LogError(ex, $@"Failed to send email to malformed RECIPIENT email ""{to}""");
				return false;
			}

			msg.Subject = subject;
			msg.Body = new TextPart(TextFormat.Html) {
				Text = renderedEmail,
			};

			return await this.SendMessageAsync(msg, cancellationToken);
		}


		/// <inheritdoc/>
		public async Task<bool> SendMessageFromResourceAsync(string to, string subject, string templateResourcePath, IDictionary<string, object> emailData, CancellationToken cancellationToken)
		{
			// Error checking: check for null arguments
			if (templateResourcePath == null)
				throw new ArgumentNullException(nameof(templateResourcePath));

			// Try to retrieve the HTML body template for the email, and then send it
			string htmlBodyTemplate = await _embeddedResourcesService.GetResourceAsStringAsync(templateResourcePath);
			if (htmlBodyTemplate == null)
				return false;
			return await SendMessageFromStringAsync(to, subject, htmlBodyTemplate, emailData, cancellationToken);
		}
	}
}