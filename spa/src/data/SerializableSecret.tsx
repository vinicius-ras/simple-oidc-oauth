/** A type representing a secret in a serializable format. */
class SerializableSecret {
	/** An optional description for the secret. */
	description?: string;
	/** The secret's actual value ("password"). */
	value?: string;
	/** The secret's expiration date and time. */
	expiration?: Date;
	/** The type of the secret. Secret types supported by IdentityServer4 can be found at the {@link IdentityServerConstants} class. */
	type?: string;
	/** A flag indicating if the {@link value} property is either in hashed or in plaintext format. This property is transient (not saved to the database).
	 * This property will be set to true if the {@link value} property contains a hashed value, or false if it
	 * contains a plaintext secret value. A value of null indicates that the secret's value hashing state is currently indetermined - it might or might not be hashed. */
	isValueHashed?: boolean;

	/** Fixes issues caused by JSON deserialization of {@link SerializableSecret} objects, such as dates coming specified as of strings.
	 * @param {SerializableSecret} secret The secret whose data should be checked and fixed.
	 * @return Returns the same secret as passed to that function. */
	 static fixJsonDeserialization(secret: SerializableSecret): SerializableSecret {
		if (secret.expiration) {
			const expirationAsAny: any = secret.expiration;
			if (typeof(expirationAsAny) === "string")
				secret.expiration = new Date(expirationAsAny);
		}
		return secret;
	}
};

export default SerializableSecret;