using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Services;
using Xunit;

namespace SimpleOidcOauth.Tests.Unit.Services
{
    /// <summary>Tests for the <see cref="EmailService" /> class.</summary>
    public class EmailServiceTests
    {
        // CONSTANTS
        /// <summary>A fake user name to be used for SMTP authentication during the tests.</summary>
        private const string FAKE_SENDER_USER_NAME = "fake-user-name";
        /// <summary>A fake SMTP host to be used during the tests.</summary>
        private const string FAKE_SENDER_SMTP_HOST = "fake-email-host-05efe18ff7ac4458aafd792e9c6eedae.com";
        /// <summary>A fake user's email to be used during the tests.</summary>
        private const string FAKE_SENDER_EMAIL = FAKE_SENDER_USER_NAME + "@" + FAKE_SENDER_SMTP_HOST;
        /// <summary>A fake user password to be used for SMTP authentication during the tests.</summary>
        private const string FAKE_SENDER_PASSWORD = "fake-password-123456";
        /// <summary>Fake SMTP port value to be used during the tests.</summary>
        private const int FAKE_SENDER_SMTP_PORT = 25;
        /// <summary>Fake email subject to be used during the tests.</summary>
        private const string FAKE_EMAIL_SUBJECT = "fake email subject";
        /// <summary>Fake recipient to be accepted as a valid recipient during the tests.</summary>
        private const string FAKE_VALID_RECIPIENT = "fake-email-valid-recipient@recipient-fake-email-host-804ec957abdc405ab2dd217fdae28010.com";
        /// <summary>Fake recipient to be rejected as a valid recipient during the tests.</summary>
        private const string FAKE_INVALID_RECIPIENT = "fake-email-invalid-recipient@INVALID-recipient-fake-email-host-0e78ab2710c04c27865d5700a6a832d5.com";
        /// <summary>A fake, malformed email, for catching email parsing errors during the test.</summary>
        private const string FAKE_MALFORMED_EMAIL = "fake-email-invalid-recipient@...$%^";
        /// <summary>
        ///     Valid resource path to an email template, which will be used during the tests.
        ///     Notice that this value is actually a resource contained within the "auth-server-tests" project (not the main "auth-server" project).
        ///     A mock <see cref="IEmbeddedResourcesService" /> will be used and will actually load a valid email template for testing purposes.
        /// </summary>
        private const string FAKE_VALID_EMAIL_RESOURCE_PATH = "SimpleOidcOauth.Tests.Unit.EmbeddedResources.FakeEmailContents.html";
        /// <summary>A resource path that should be considered as an invalid resource path during the tests.</summary>
        private const string FAKE_INVALID_EMAIL_RESOURCE_PATH = "SimpleOidcOauth.Tests.Unit.EmbeddedResources.INVALID-FakeEmailContents.html";
        /// <summary>
        ///     This field will actually hold the email template's contents to be used during the tests.
        ///     For more information, see <see cref="FAKE_VALID_EMAIL_RESOURCE_PATH" />.
        /// </summary>
        private static readonly string FAKE_EMAIL_CONTENTS;
        /// <summary>A series of object collections, representing several valid email data to be used to render emails during the tests.</summary>
        /// <value>
        ///     <para>
        ///         This field contains a series of object collections, where each collection represents a set of data to be passed when rendering the emails.
        ///         Each collection contains elements that might be either <see cref="KeyValuePair" /> objects or <see cref="String"/> values.
        ///     </para>
        ///     <para>
        ///         The key-pair values will be passed as the email's data to be used by the <see cref="IEmailService"/>'s <c>SendMessageFrom*(...)</c> methods for rendering the emails.
        ///     </para>
        ///     <para>
        ///         The strings represent the expected texts to be contained in the rendered emails. These will usually be very specific values that can be easilly spotted in the
        ///         rendered email, like magic numbers and predefined GUID values. Some of the strings will also be set to the names of variables that are NOT present in the test collection:
        ///         as they have no correspondding <see cref="KeyValuePair" /> in the collection, they are supposed NOT to be replaced during the rendering of the emails.
        ///     </para>
        /// </value>
        public static readonly object[][] FAKE_EMAIL_DATA_DESCRIPTORS = new object[][] {
            new object [] {
                KeyValuePair.Create<string, object>("fistVal", 10203349),
                KeyValuePair.Create<string, object>("_fourthVal", 1948673),
                KeyValuePair.Create<string, object>("FiFtHvAl_", 9182835),
                KeyValuePair.Create<string, object>("sIxThVaL_", 13579),
                KeyValuePair.Create<string, object>("_8th_val", 24680),
                KeyValuePair.Create<string, object>("vaL_10", 111223445),
                "10203349",
                "1948673",
                "9182835",
                "13579",
                "24680",
                "111223445",
                "${sec-val}",
                "${Third Val}",
                "${7th_val}",
            },
            new object [] {
                KeyValuePair.Create<string, object>("FiFtHvAl_", "f32e9bd05c264fcd9d03b21a4572e32f"),
                KeyValuePair.Create<string, object>("_8th_val", "19675f305d564700919a133bde0ccdf9"),
                KeyValuePair.Create<string, object>("sec-val", "15def4bec5564b49834b03de4817be28"),
                KeyValuePair.Create<string, object>("Third Val", "3f360516f5c24d4caefb6d6ef5073de6"),
                KeyValuePair.Create<string, object>("7th_val", "9faa38076f6845b0bcaf5b075ad2492a"),
                "f32e9bd05c264fcd9d03b21a4572e32f",
                "19675f305d564700919a133bde0ccdf9",
                "${fistVal}",
                "${_fourthVal}",
                "${sIxThVaL_}",
                "${vaL_10}",
                "${sec-val}",
                "${Third Val}",
                "${7th_val}",
            },
            new object [] {
                "${fistVal}",
                "${sec-val}",
                "${Third Val}",
                "${_fourthVal}",
                "${FiFtHvAl_}",
                "${sIxThVaL_}",
                "${7th_val}",
                "${_8th_val}",
                "${vaL_10}",
            },
        };





        // STATIC METHODS
        /// <summary>Static initializer.</summary>
        static EmailServiceTests()
        {
            var thisTypeAssembly = typeof(EmailServiceTests).Assembly;
            using (var resourceStream = thisTypeAssembly.GetManifestResourceStream(FAKE_VALID_EMAIL_RESOURCE_PATH))
            using (var stringReader = new StreamReader(resourceStream))
                FAKE_EMAIL_CONTENTS = stringReader.ReadToEnd();
        }




        // INSTANCE METHODS
        /// <summary>Creates a default mock SMTP client to be used in the tests.</summary>
        /// <returns>Returns a new instance of a default mock SMTP client to be used in the tests.</returns>
        private Mock<ISmtpClient> GetDefaultMockSmtpClient()
        {
            var mockSmtpClient = new Mock<ISmtpClient>();

            mockSmtpClient.Setup(client =>
                client.ConnectAsync(
                    FAKE_SENDER_SMTP_HOST,
                    FAKE_SENDER_SMTP_PORT,
                    It.IsAny<SecureSocketOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockSmtpClient.Setup(client =>
                client.ConnectAsync(
                    It.Is<string>(str => string.IsNullOrWhiteSpace(str)),
                    It.IsAny<int>(),
                    It.IsAny<SecureSocketOptions>(),
                    It.IsAny<CancellationToken>()))
                .Throws<UriFormatException>();
            mockSmtpClient.Setup(client =>
                client.ConnectAsync(
                    It.Is<string>(str => str == string.Empty),
                    It.IsAny<int>(),
                    It.IsAny<SecureSocketOptions>(),
                    It.IsAny<CancellationToken>()))
                .Throws<ArgumentException>();
            mockSmtpClient.Setup(client =>
                client.ConnectAsync(
                    null,
                    It.IsAny<int>(),
                    It.IsAny<SecureSocketOptions>(),
                    It.IsAny<CancellationToken>()))
                .Throws<ArgumentNullException>();
            mockSmtpClient.Setup(client =>
                client.ConnectAsync(
                    It.IsAny<string>(),
                    It.Is<int>(i => i < 0 || i > UInt16.MaxValue),
                    It.IsAny<SecureSocketOptions>(),
                    It.IsAny<CancellationToken>()))
                .Throws<ArgumentOutOfRangeException>();
            mockSmtpClient.Setup(client =>
                client.ConnectAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<SecureSocketOptions>(),
                    It.Is<CancellationToken>(token => token.IsCancellationRequested)))
                .Throws<OperationCanceledException>();
            mockSmtpClient.Setup(client =>
                client.DisconnectAsync(
                    It.IsAny<bool>(),
                    It.Is<CancellationToken>(token => token.IsCancellationRequested)))
                .Throws<OperationCanceledException>();
            mockSmtpClient.Setup(client =>
                client.SendAsync(
                    It.Is<MimeMessage>(msg => msg.To.Count == 1 && msg.To.First().Name.Equals(FAKE_VALID_RECIPIENT)),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<ITransferProgress>()))
                .Returns(Task.CompletedTask);
            mockSmtpClient.Setup(client =>
                client.SendAsync(
                    It.Is<MimeMessage>(msg => msg.To.Count != 1 || (msg.To.FirstOrDefault() != null && msg.To.FirstOrDefault().Name.Equals(FAKE_INVALID_RECIPIENT))),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<ITransferProgress>()))
                .Returns(Task.CompletedTask);

            return mockSmtpClient;
        }


        /// <summary>Creates a default mock <see cref="IEmbeddedResourcesService" /> object to be used in the tests.</summary>
        /// <returns>Returns a new instance of a default mock <see cref="IEmbeddedResourcesService" /> object to be used in the tests.</returns>
        private Mock<IEmbeddedResourcesService> GetDefaultMockIEmbeddedResourcesService()
        {
            var mockResourcesService = new Mock<IEmbeddedResourcesService>();
            mockResourcesService.Setup(resourcesSvc =>
                resourcesSvc.GetResourceAsStringAsync(
                    FAKE_VALID_EMAIL_RESOURCE_PATH,
                    It.IsAny<Encoding>()))
                .ReturnsAsync(FAKE_EMAIL_CONTENTS);
            mockResourcesService.Setup(resourcesSvc =>
                resourcesSvc.GetResourceAsStringAsync(
                    FAKE_INVALID_EMAIL_RESOURCE_PATH,
                    It.IsAny<Encoding>()))
                .ReturnsAsync((string)null);
            return mockResourcesService;
        }


        /// <summary>Creates a default mock <see cref="IOptions{TOptions}" /> wrapping a default <see cref="AppConfigs" /> object to be used in the tests.</summary>
        /// <returns>Returns a new instance of a default mock <see cref="IOptions{TOptions}" /> wrapping a default <see cref="AppConfigs" /> object to be used in the tests.</returns>
        private IOptions<AppConfigs> GetDefaultAppConfigsOptions() => Options.Create(
            new AppConfigs
            {
                Email = new EmailConfigs
                {
                    Host = FAKE_SENDER_SMTP_HOST,
                    DefaultSenderEmail = FAKE_SENDER_EMAIL,
                    UserName = FAKE_SENDER_USER_NAME,
                    Password = FAKE_SENDER_PASSWORD,
                    Port = FAKE_SENDER_SMTP_PORT,
                }
            }
        );





        // TESTS
        [Fact]
        public async Task SendMessageFromStringAsync_ValidEmailData_ReturnsTrue() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);

            Assert.True(sendResult);
        }


        [Fact]
        public async Task SendMessageFromStringAsync_MalformedSenderEmail_ReturnsFalse() {
            var appConfigs = GetDefaultAppConfigsOptions();
            appConfigs.Value.Email.DefaultSenderEmail = FAKE_MALFORMED_EMAIL;

            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);

            Assert.False(sendResult);
        }


        [Fact]
        public async Task SendMessageFromStringAsync_NullSenderEmail_ReturnsFalse() {
            var appConfigs = GetDefaultAppConfigsOptions();
            appConfigs.Value.Email.DefaultSenderEmail = null;

            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS));
        }


        [Fact]
        public async Task SendMessageFromStringAsync_MalformedRecipientEmail_ReturnsFalse() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_MALFORMED_EMAIL, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);

            Assert.False(sendResult);
        }


        [Fact]
        public async Task SendMessageFromStringAsync_NullRecipientEmail_ThrowsArgumentNullException() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => emailService.SendMessageFromStringAsync(null, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS));
        }


        [Fact]
        public async Task SendMessageFromStringAsync_NullEmailSubject_ThrowsArgumentNullException() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, null, FAKE_EMAIL_CONTENTS));
        }


        [Fact]
        public async Task SendMessageFromStringAsync_NullEmailContents_ThrowsArgumentNullException() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, null));
        }


        [Fact]
        public async Task SendMessageFromStringAsync_NullHostValue_ReturnsFalse() {
            var appConfigs = GetDefaultAppConfigsOptions();
            appConfigs.Value.Email.Host = null;

            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);

            Assert.False(sendResult);
        }


        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        [InlineData("\r\n")]
        [InlineData("\t\r\n")]
        [InlineData("\r\n\t")]
        public async Task SendMessageFromStringAsync_EmptyHostValue_ReturnsFalse(string host) {
            var appConfigs = GetDefaultAppConfigsOptions();
            appConfigs.Value.Email.Host = host;

            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);

            Assert.False(sendResult);
        }


        [Theory]
        [InlineData(-1)]
        [InlineData(-7)]
        [InlineData(-100)]
        public async Task SendMessageFromStringAsync_NegativePortValue_ReturnsFalse(int portValue) {
            var appConfigs = GetDefaultAppConfigsOptions();
            appConfigs.Value.Email.Port = portValue;

            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);

            Assert.False(sendResult);
        }


        [Theory]
        [InlineData(80000)]
        [InlineData(999999)]
        [InlineData(UInt16.MaxValue + 1)]
        public async Task SendMessageFromStringAsync_PortValueHigherThanMaximumUInt16_ReturnsFalse(int portValue) {
            var appConfigs = GetDefaultAppConfigsOptions();
            appConfigs.Value.Email.Port = portValue;

            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);

            Assert.False(sendResult);
        }


        [Fact]
        public async Task SendMessageFromStringAsync_SmtpClientConnectAsyncIOException_ReturnsFalse() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();

            var smtpClientMock = GetDefaultMockSmtpClient();
            smtpClientMock.Setup(client =>
                client.ConnectAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<SecureSocketOptions>(),
                    It.IsAny<CancellationToken>()
                )).Throws<IOException>();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS);
            Assert.False(sendResult);
        }


        [Fact]
        public async Task SendMessageFromStringAsync_OperationCancelledBeforeStarting_ThrowsOperationCancelledException() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var cancelledToken = new CancellationToken(true);
            await Assert.ThrowsAsync<OperationCanceledException>(() => emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS, cancellationToken: cancelledToken));
        }


        [Theory]
        [MemberData(nameof(FAKE_EMAIL_DATA_DESCRIPTORS))]
        public async Task SendMessageFromStringAsync_EmailDataChanges_ChangesEmailBody(params object [] testData) {
            var emailDataKeyValuePairs = testData.OfType<KeyValuePair<string, object>>().ToList();
            var expectedOutputs = testData.OfType<string>().ToList();

            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();

            string sentMessageBody = null;
            var smtpClientMock = GetDefaultMockSmtpClient();
            smtpClientMock.Setup(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
                .Callback((MimeMessage msg, CancellationToken _, ITransferProgress _) => sentMessageBody = msg.HtmlBody);

            IEmailService emailService = new EmailService(appConfigs, logger.Object, null, smtpClientMock.Object);

            var emailDataDictionary = new Dictionary<string, object>(emailDataKeyValuePairs);
            var sendResult = await emailService.SendMessageFromStringAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_EMAIL_CONTENTS, emailDataDictionary);

            Assert.Equal(testData.Length, emailDataKeyValuePairs.Count + expectedOutputs.Count);
            foreach (var expectedOutput in expectedOutputs)
                Assert.Contains(expectedOutput, sentMessageBody);
        }


        [Fact]
        public async Task SendMessageFromResourceAsync_ValidResourcePath_ReturnsTrue() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            var resourcesSvc = GetDefaultMockIEmbeddedResourcesService();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, resourcesSvc.Object, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromResourceAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_VALID_EMAIL_RESOURCE_PATH);
            Assert.True(sendResult);
        }


        [Fact]
        public async Task SendMessageFromResourceAsync_InvalidResourcePath_ReturnsFalse() {
            var appConfigs = GetDefaultAppConfigsOptions();
            var logger = new Mock<ILogger<EmailService>>();
            var smtpClientMock = GetDefaultMockSmtpClient();
            var resourcesSvc = GetDefaultMockIEmbeddedResourcesService();
            IEmailService emailService = new EmailService(appConfigs, logger.Object, resourcesSvc.Object, smtpClientMock.Object);

            var sendResult = await emailService.SendMessageFromResourceAsync(FAKE_VALID_RECIPIENT, FAKE_EMAIL_SUBJECT, FAKE_INVALID_EMAIL_RESOURCE_PATH);
            Assert.False(sendResult);
        }
    }
}
