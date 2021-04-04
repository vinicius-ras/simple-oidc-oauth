/** Models some of the values contained in the IdentityServer4's homonymous class. */
export default abstract class IdentityServerConstants {
	/** The types of secrets supported by IdentityServer4. */
	public static SecretTypes = class {
		public static readonly SharedSecret = "SharedSecret";
		public static readonly X509CertificateThumbprint = "X509Thumbprint";
		public static readonly X509CertificateName = "X509Name";
		public static readonly X509CertificateBase64 = "X509CertificateBase64";
		public static readonly JsonWebKey = "JWK";
	}
}